#if !UNITY_EDITOR && UNITY_WSA
namespace MediaFrameQrProcessing.VideoDeviceFinders
{

  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.Devices.Enumeration;

  public static class VideoCaptureDeviceFinder
  {
    public static async Task<DeviceInformation> FindFirstOrDefaultAsync()
    {
      var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
      return (devices.FirstOrDefault());
    }
    public static async Task<DeviceInformation> FindAsync(
      Func<DeviceInformationCollection, DeviceInformation> filter)
    {
      var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
      return (filter(devices));
    }
  }

}

#endif
