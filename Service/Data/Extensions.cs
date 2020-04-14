using NSPersonalCloud.FileSharing.Aliyun;

using Unishare.Apps.Common.Models;

namespace Unishare.Apps.WindowsService.Data
{
    public static class Extensions
    {
        public static OssConfig ToConfig(this AliYunOSS model)
        {
            return new OssConfig {
                OssEndpoint = model.Endpoint,
                BucketName = model.Bucket,
                AccessKeyId = model.AccessID,
                AccessKeySecret = model.AccessSecret
            };
        }

        public static AliYunOSS ToModel(this OssConfig config, string name)
        {
            return new AliYunOSS {
                Name = name,
                Endpoint = config.OssEndpoint,
                Bucket = config.BucketName,
                AccessID = config.AccessKeyId,
                AccessSecret = config.AccessKeySecret
            };
        }
    }
}
