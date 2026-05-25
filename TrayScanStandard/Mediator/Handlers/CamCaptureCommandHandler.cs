using Camera.Fs.Common;
using HKCamera.Fs.NET.Controls;
using LinxUniverse.Utils;
using MediatR;
using MugenCamera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrayScanStandard.Mediator.Commands;
using TrayScanStandard.Utils;

namespace TrayScanStandard.Mediator.Handlers
{
    // 此处用于处理相机拍照的命令
    public class CamCaptureCommandHandler : IRequestHandler<CamCaptureCommand, Either<string, IEnumerable<Image2DResult[]>>>
    {
        public Task<Either<string, IEnumerable<Image2DResult[]>>> Handle(CamCaptureCommand request, CancellationToken cancellationToken)
        {



            //var a = request.CaptureInfos.Select(s => 1);
            if (!MainStorage.Saves.CameraEnable)
            {
                return Either<string, IEnumerable<Image2DResult[]>>.Right
               ([..request.CaptureInfos
               .Select((s, i) => new Image2DResult[]
                       {
                           new Image2DResult(File.ReadAllBytes( @$"testImg\{i}.png"))
                           //new Image2DResult(File.ReadAllBytes( @$"C:\Users\admin\Pictures\20211110085254_736a4.jpeg"))
                       }
                 )
               ]).Apply(Task.FromResult);
            }

            // 启用相机拍照，使用 LightManagerViewModel 中的光源设置
            return DetectUtil.UseLight(
                () => request.CaptureInfos
                    .Map(s => s.ToEither("相机未初始化"))
                    .Traverse(s => s)
                    .Bind(c =>
                    #region
                    //.BindAsync(c =>
                    //    c
                    //    //.AsParallel()    //华睿相机并行拍照会有问题，暂时不使用
                    //    // .AsOrdered()
                    //
                    //
                    //    //.Select(s => Task.Run(() => ProcessCaptureInfo(s)))
                    //    //.Apply(Task.WhenAll)
                    //    //.Map(s => s.Traverse(q => q))
                    //
                    //    //.Select(s => new Func<Either<string, Image2DResult[]>>(() => ProcessCaptureInfo(s)))
                    //    //.Map(s => s.Invoke())
                    //    .Select(ProcessCaptureInfo)
                    //    .Traverse(s => s)
                    //
                    //
                    //    //.Select(ProcessCaptureInfo)
                    //    //.Traverse(s =>s)
                    //    )
                    #endregion
                    {
                        var captureInfos = c.ToArray();
                        var results = new Either<string, Image2DResult[]>[captureInfos.Length];

                        // 海康与其他分支沿用原逻辑：顺序执行，保持既有行为不变。
                        captureInfos
                            .Select((info, idx) => (info, idx))
                            .Where(x => x.info.Camera is not HuaruiCam)
                            .AsParallel()
                            .Iter(x =>
                            {
                                results[x.idx] = ProcessCaptureInfo(x.info);
                            });

                        // 仅华睿分支并行拍照，再按原索引回填，避免图像槽位互换。
                        captureInfos
                            .Select((info, idx) => (info, idx))
                            .Where(x => x.info.Camera is HuaruiCam)
                            .AsParallel()
                            .Select(x => (x.idx, result: ProcessCaptureInfo(x.info)))
                            .ToArray()
                            .Iter(x =>
                            {
                                results[x.idx] = x.result;
                            });

                        return results.Traverse(s => s);
                    })
                    //.TraverseParallel(s => s)
                    )
                .Apply(Task.FromResult);

            // 本地函数：处理单个CaptureInfo
            Either<string, Image2DResult[]> ProcessCaptureInfo(CaptureInfo s)
            {
                var aa = s.Exps.Map(e =>
                        s.Camera
                        .SetControl(new AcquisitionControl { ExposureTime = e })
                        .Bind(DetectUtil.CaptureOne)
                    )
                .Traverse(s => s)
                .Map(s => s.ToArray());
                return aa;
            }
        }
    }

}
