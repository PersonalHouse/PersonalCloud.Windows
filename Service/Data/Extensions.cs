using NSPersonalCloud.FileSharing.Aliyun;

using NSPersonalCloud.Common.Models;

namespace NSPersonalCloud.WindowsService.Data
{
    public static class Extensions
    {
        public static OssConfig ToConfig(this AlibabaOSS model)
        {
            return new OssConfig {
                OssEndpoint = model.Endpoint,
                BucketName = model.Bucket,
                AccessKeyId = model.AccessID,
                AccessKeySecret = model.AccessSecret
            };
        }

        public static AlibabaOSS ToModel(this OssConfig config, string name)
        {
            return new AlibabaOSS {
                Name = name,
                Endpoint = config.OssEndpoint,
                Bucket = config.BucketName,
                AccessID = config.AccessKeyId,
                AccessSecret = config.AccessKeySecret
            };
        }
    }
}
