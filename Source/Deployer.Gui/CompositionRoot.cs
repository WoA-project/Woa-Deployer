﻿using System;
using Deployer.Core;
using Deployer.Core.Interaction;
using Deployer.Core.Requirements;
using Deployer.Core.Scripting;
using Deployer.Core.Services;
using Deployer.Gui.Services;
using Deployer.Gui.ViewModels.Common;
using Deployer.Gui.ViewModels.RequirementSolvers;
using Deployer.NetFx;
using Deployer.Wpf;
using DiscUtils;
using Grace.DependencyInjection;
using Serilog.Events;
using Zafiro.Core;
using Zafiro.Core.Files;
using Zafiro.Core.FileSystem;
using Zafiro.Core.Net4x;
using Zafiro.UI;
using Zafiro.UI.Wpf;
using Commands = Deployer.Wpf.Commands;
using DeployerFileOpenService = Deployer.Wpf.DeployerFileOpenService;
using DiskRequirementSolver = Deployer.Wpf.DiskRequirementSolver;
using IFileSystem = Deployer.Filesystem.IFileSystem;
using MarkdownService = Deployer.Wpf.MarkdownService;
using WimPickRequirementSolver = Deployer.Wpf.WimPickRequirementSolver;

namespace Deployer.Gui
{
    public class CompositionRoot
    {
        public static DependencyInjectionContainer CreateContainer()
        {
            var container = new DependencyInjectionContainer();
            container.Configure(block =>
            {
                block.Export<FileSystemOperations>().As<IFileSystemOperations>().Lifestyle.Singleton();
                block.Export<RequirementSupplier>().As<IRequirementSupplier>().Lifestyle.Singleton();
                block.Export<IridioRequirementsAnalyzer>().As<IRequirementsAnalyzer>().Lifestyle.Singleton();
                block.Export<RequirementsManager>().As<IRequirementsManager>().Lifestyle.Singleton();
                block.ExportFactory((IRequirementsManager rm, IFileSystem fs) => new WoaDeployer(x =>
                {
                    x.ExportFactory(() => rm).As<IRequirementsManager>();
                    x.ExportFactory(() => fs).As<IFileSystem>();
                })).Lifestyle.Singleton();
                block.ExportFactory((WoaDeployer wd) => wd.OperationContext).As<IOperationContext>().Lifestyle.Singleton();
                block.ExportFactory((WoaDeployer wd) => wd.OperationProgress).As<IOperationProgress>().Lifestyle.Singleton();
                block.Export<OpenFilePicker>().As<IOpenFilePicker>().Lifestyle.Singleton();
                block.ExportFactory<Uri, IFileSystemOperations, IZafiroFile>((uri, f) => new ZafiroFile(uri, f, null));
                block.Export<SettingsService>().As<ISettingsService>().Lifestyle.Singleton();
                block.Export<MarkdownService>().As<IMarkdownService>().Lifestyle.Singleton();
                block.ExportAssemblies(new[] { typeof(ViewModels.Sections.MainViewModel).Assembly })
                    .Where(y =>
                    {
                        var isSection = typeof(ISection).IsAssignableFrom(y);
                        return isSection;
                    })
                    .ByInterface<ISection>()
                    .ByInterface<IBusy>()
                    .ByType()
                    .ExportAttributedTypes()
                    .Lifestyle.Singleton();
                block.Export<PopupWindow>().As<IPopup>();
                block.Export<Popup>().As<IPopup>();
                block.ExportFactory<string, IHaveDataContext>(_ => new HaveDataContextAdapter(new Views.Requirements()));
                block.ExportFactory<string, IFileSystemOperations, IDownloader, IZafiroFile>((s, fso, dl) =>
                    new ZafiroFile(new Uri(s), fso, dl));

                block.Export<WimPickRequirementSolver>().Lifestyle.Singleton();
                block.ExportFactory<ResolveSettings, Commands, DeployerFileOpenService, IFileSystem, IRequirementSolver>((settings, commands, fileOpenService, fs) =>
                {
                    if (settings.Kind == RequirementKind.WimFile)
                    {
                        return new WimPickRequirementSolver(settings.Key, commands, fileOpenService);
                    }

                    if (settings.Kind == RequirementKind.Disk)
                    {
                        return new DiskRequirementSolver(settings.Key, fs);
                    }

                    throw new ArgumentOutOfRangeException();
                });

                block.ExportFactory((IDownloader downloader) => XmlDeviceRepository(downloader)).As<IDevRepo>().Lifestyle
                    .Singleton();

                block.Export<Downloader>().As<IDownloader>().Lifestyle.Singleton();
                //block.ExportFactory(() => LogEventSource.Current).As<IObservable<LogEvent>>().Lifestyle.Singleton();
                block.Export<ShellOpen>().As<IShellOpen>().Lifestyle.Singleton();
                block.Export<FileSystem>().As<IFileSystem>().Lifestyle.Singleton();
            });

            return container;
        }

        private static XmlDeviceRepository XmlDeviceRepository(IDownloader downloader)
        {
            var definition = "https://raw.githubusercontent.com/WOA-Project/Deployment-Feed/master/Deployments.xml";
            return new XmlDeviceRepository(new Uri(definition), downloader);
        }

    }
}