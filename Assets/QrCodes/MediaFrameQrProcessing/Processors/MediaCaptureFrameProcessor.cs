namespace MediaFrameQrProcessing.Processors
{
    #if !UNITY_EDITOR && UNITY_WSA
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using VideoDeviceFinders;
    using Windows.Devices.Enumeration;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;
    using Windows.Media.Devices;

    public abstract class MediaCaptureFrameProcessor<T> : IDisposable
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        Action<VideoDeviceController> videoDeviceControllerInitialiser;
        readonly string mediaEncodingSubtype;
        readonly MediaFrameSourceFinder mediaFrameSourceFinder;
        readonly DeviceInformation videoDeviceInformation;
        readonly MediaCaptureMemoryPreference memoryPreference;
        MediaCapture mediaCapture;

        public MediaCaptureFrameProcessor(
      MediaFrameSourceFinder mediaFrameSourceFinder,
      DeviceInformation videoDeviceInformation,
      string mediaEncodingSubtype,
      MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu)
        {
            this.mediaFrameSourceFinder = mediaFrameSourceFinder;
            this.videoDeviceInformation = videoDeviceInformation;
            this.mediaEncodingSubtype = mediaEncodingSubtype;
            this.memoryPreference = memoryPreference;
        }
        public void SetVideoDeviceControllerInitialiser(
          Action<VideoDeviceController> initialiser)
        {
            this.videoDeviceControllerInitialiser = initialiser;
        }
        protected abstract bool ProcessFrame(MediaFrameReference frameReference);
        public T Result { get; protected set; }

        public async Task ProcessFramesAsync(
            CancellationToken token,
            Action<T> resultCallback = null)
        {
            // Note: the natural thing to do here is what I used to do which is to create the
            // MediaCapture inside of a using block.
            // Problem is, that seemed to cause a situation where I could get a crash (AV) in
            //
            // Windows.Media.dll!Windows::Media::Capture::Frame::MediaFrameReader::CompletePendingStopOperation
            //
            // Which seemed to be related to stopping/disposing the MediaFrameReader and then
            // disposing the media capture immediately after.
            // 
            // Right now, I've promoted the media capture to a member variable and held it around
            // and instead of creating/disposing an instance each time one instance is kept
            // indefinitely.
            // It's not what I wanted...
            await Task.Run(
              async () =>
              {
                  var startTime = DateTime.Now;

                  if (this.mediaCapture == null)
                  {
                      this.mediaCapture = await this.CreateMediaCaptureAsync();
                  }
                  var mediaFrameSource = this.mediaCapture.FrameSources[this.mediaFrameSourceFinder.FrameSourceInfo.Id];

                  using (var frameReader = await this.mediaCapture.CreateFrameReaderAsync(mediaFrameSource, this.mediaEncodingSubtype))
                  {


                      await frameReader.StartAsync();

                      while (!token.IsCancellationRequested)
                      {
                          using (var frame = frameReader.TryAcquireLatestFrame())
                          {
                              if ((frame != null) && (this.ProcessFrame(frame) && (resultCallback != null)))
                              {
                                      resultCallback(this.Result);
                              }
                       
                          }
                      }
                      await frameReader.StopAsync();
                  }
              }
            );
        }
        async Task<MediaCapture> CreateMediaCaptureAsync()
        {
            var settings = new MediaCaptureInitializationSettings()
            {
                VideoDeviceId = this.videoDeviceInformation.Id,
                SourceGroup = this.mediaFrameSourceFinder.FrameSourceGroup,
                MemoryPreference = this.memoryPreference
            };

            var mediaCapture = new MediaCapture();

            await mediaCapture.InitializeAsync(settings);

            this.videoDeviceControllerInitialiser?.Invoke(mediaCapture.VideoDeviceController);

            return (mediaCapture);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (this.mediaCapture != null)
                {
                    this.mediaCapture.Dispose();
                    this.mediaCapture = null;
                }
            }

            disposed = true;
        }

        

    }
#endif
}