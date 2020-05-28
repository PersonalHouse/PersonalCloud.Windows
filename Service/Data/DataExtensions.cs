using System;

using NSPersonalCloud.Common.Models;
using NSPersonalCloud.FileSharing.Aliyun;

namespace NSPersonalCloud.WindowsService.Data
{
    public static class DataExtensions
    {
        public static OssConfig ToConfig(this AlibabaOSS model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            return new OssConfig {
                OssEndpoint = model.Endpoint,
                BucketName = model.Bucket,
                AccessKeyId = model.AccessID,
                AccessKeySecret = model.AccessSecret
            };
        }

        public static AlibabaOSS ToModel(this OssConfig config, string name)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));

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
