#if !UNITY_EDITOR && UNITY_WSA

namespace MediaFrameQrProcessing.VideoDeviceFinders
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using Windows.Media.Capture;
  using Windows.Media.Capture.Frames;

  public class MediaFrameSourceFinder
  {
    public MediaFrameSourceGroup FrameSourceGroup { get; private set; }
    public MediaFrameSourceInfo FrameSourceInfo { get; private set; }
    public MediaFrameSourceFinder()
    {

    }
    public async Task<bool> PopulateAsync(
      Func<MediaFrameSourceInfo, bool> sourceInfoFilter,
      Func<IEnumerable<MediaFrameSourceGroup>, MediaFrameSourceGroup> sourceGroupSelector)
    {
      var mediaFrameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
            //
            // var candidates = mediaFrameSourceGroups.Where(
            //   group => group.SourceInfos.Any(sourceInfoFilter));
            var candidates = mediaFrameSourceGroups.Where(x => true);
      this.FrameSourceGroup = sourceGroupSelector(candidates);

            // this.FrameSourceInfo = this.FrameSourceGroup?.SourceInfos.FirstOrDefault(
            // sourceInfoFilter);
            this.FrameSourceInfo = this.FrameSourceGroup?.SourceInfos.FirstOrDefault();
      return ((this.FrameSourceGroup != null) && (this.FrameSourceInfo != null));
    }
    public static MediaFrameSourceGroup FirstOrDefault(
      IEnumerable<MediaFrameSourceGroup> groups)
    {
      return (groups.FirstOrDefault());
    }
    public static bool ColorVideoPreviewFilter(MediaFrameSourceInfo sourceInfo)
    {
      return (
        (sourceInfo.MediaStreamType == MediaStreamType.VideoPreview) &&
        (sourceInfo.SourceKind == MediaFrameSourceKind.Color));
    }
  }
}

#endif