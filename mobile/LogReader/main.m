#import <UIKit/UIKit.h>
#import "Scanner.h"

@interface MJReaderController : UITableViewController
@property(nonatomic,strong) NSArray<NSDictionary *> *files;
@property(nonatomic,strong) NSMutableIndexSet *selected;
@property(nonatomic,strong) NSDictionary *report;
@property(nonatomic,strong) NSArray<NSString *> *statusLines;
@property(nonatomic,strong) UIBarButtonItem *exportButton;
@property(nonatomic,strong) UIBarButtonItem *completeButton;
@property(nonatomic,strong) UIBarButtonItem *clearButton;
@property(nonatomic,copy) NSString *selectionMode;
@property(nonatomic,strong) NSArray<NSDictionary *> *selectionIssues;
@property(nonatomic,assign) BOOL busy;
@end

@implementation MJReaderController
- (void)viewDidLoad {
    [super viewDidLoad];
    self.title=@"麻将日志导出";
    self.files=@[]; self.selected=[NSMutableIndexSet indexSet];
    self.selectionMode=@"manual"; self.selectionIssues=@[];
    self.statusLines=@[@"已有对局日志可直接点击“扫描”，无需重新打一局。", @"扫描后点底部“完整选择”，将现有候选日志、配置和补丁一起导出。", @"本工具读取现有文件；日志未必包含原始对局消息。"];
    self.navigationItem.leftBarButtonItem=[[UIBarButtonItem alloc] initWithTitle:@"扫描" style:UIBarButtonItemStylePlain target:self action:@selector(scan)];
    self.exportButton=[[UIBarButtonItem alloc] initWithTitle:@"导出" style:UIBarButtonItemStylePlain target:self action:@selector(exportFiles)];
    self.exportButton.enabled=NO; self.navigationItem.rightBarButtonItem=self.exportButton;
    self.tableView.estimatedRowHeight=72; self.tableView.rowHeight=UITableViewAutomaticDimension;
    self.tableView.accessibilityIdentifier=@"log-reader-table";
    self.completeButton=[[UIBarButtonItem alloc] initWithTitle:@"完整选择" style:UIBarButtonItemStylePlain target:self action:@selector(selectComplete)];
    self.clearButton=[[UIBarButtonItem alloc] initWithTitle:@"清空选择" style:UIBarButtonItemStylePlain target:self action:@selector(clearSelection)];
    UIBarButtonItem *space=[[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil];
    self.toolbarItems=@[self.completeButton,space,self.clearButton];
    self.completeButton.enabled=NO; self.clearButton.enabled=NO;
    [self.navigationController setToolbarHidden:NO animated:NO];
}
- (void)showMessage:(NSString *)message {
    UIAlertController *alert=[UIAlertController alertControllerWithTitle:@"麻将日志导出" message:message preferredStyle:UIAlertControllerStyleAlert];
    [alert addAction:[UIAlertAction actionWithTitle:@"好" style:UIAlertActionStyleDefault handler:nil]];
    [self presentViewController:alert animated:YES completion:nil];
}
- (void)setWorking:(BOOL)value {
    self.busy=value; self.navigationItem.leftBarButtonItem.enabled=!value;
    self.exportButton.enabled=!value && self.report!=nil;
    self.completeButton.enabled=!value && self.report!=nil && self.files.count>0;
    self.clearButton.enabled=!value && self.selected.count>0;
}
- (void)refreshSelection {
    self.exportButton.title=[NSString stringWithFormat:@"导出(%lu)",(unsigned long)self.selected.count];
    [self setWorking:self.busy]; [self.tableView reloadData];
}
- (void)selectComplete {
    if (self.busy || !self.report) return;
    NSDictionary *plan=MJPlanCompleteSelection(self.files);
    [self.selected removeAllIndexes];
    for (NSNumber *index in plan[@"indexes"]) [self.selected addIndex:index.unsignedIntegerValue];
    self.selectionMode=@"complete_candidates"; self.selectionIssues=plan[@"skipped"];
    [self refreshSelection];
    if (self.selectionIssues.count) [self showMessage:[NSString stringWithFormat:@"已选择 %lu/%lu 项；%lu 项因单文件或总大小限制未选入。导出清单会记录这些项目，文件列表仍可手动调整。",(unsigned long)self.selected.count,(unsigned long)self.files.count,(unsigned long)self.selectionIssues.count]];
}
- (void)clearSelection {
    if (self.busy) return;
    [self.selected removeAllIndexes]; self.selectionMode=@"manual"; self.selectionIssues=@[]; [self refreshSelection];
}
- (void)scan {
    if (self.busy) return;
    [self setWorking:YES];
    self.statusLines=@[@"正在查找欢乐麻将容器并读取文件清单……"];
    [self.tableView reloadData];
    dispatch_async(dispatch_get_global_queue(QOS_CLASS_USER_INITIATED,0), ^{
        NSMutableArray *errors=[NSMutableArray array];
        NSArray *containers=MJFindContainers(@"/var/mobile/Containers/Data/Application",errors);
        NSMutableArray *scans=[NSMutableArray array], *files=[NSMutableArray array];
        for (NSString *container in containers) {
            NSDictionary *scan=MJScanContainer(container); [scans addObject:scan]; [files addObjectsFromArray:scan[@"files"]];
        }
        NSDictionary *version=MJFindAppVersion(@"/var/containers/Bundle/Application");
        NSDictionary *report=@{@"schema":@2, @"tool_version":[NSBundle.mainBundle objectForInfoDictionaryKey:@"CFBundleShortVersionString"] ?: @"1.1", @"bundle_id":@"com.tencent.happymj", @"device_system":UIDevice.currentDevice.systemVersion,
            @"created":@([NSDate.date timeIntervalSince1970]), @"app_version":version, @"scans":scans, @"errors":errors,
            @"scope":@"Existing local files only. No process injection, memory capture, network capture, or server requests.",
            @"interpretation":@"A filename or replay file does not prove live opponents hands are available. Some logs are encrypted or lack message bodies."};
        dispatch_async(dispatch_get_main_queue(), ^{
            self.report=report; self.files=files; [self.selected removeAllIndexes];
            self.selectionMode=@"manual"; self.selectionIssues=@[];
            self.exportButton.title=@"导出(0)";
            NSMutableArray *status=[NSMutableArray arrayWithObjects:[NSString stringWithFormat:@"找到 %lu 个欢乐麻将容器、%lu 个候选文件。",(unsigned long)containers.count,(unsigned long)files.count],nil];
            if (version.count) [status addObject:[NSString stringWithFormat:@"游戏版本 %@（%@）",version[@"version"],version[@"build"]]];
            if (!containers.count) [status addObject:@"未找到原包名 com.tencent.happymj 的容器，或访问被系统拒绝。请导出清单以便检查；分身或改包名版本不会被自动扫描。"];
            if (containers.count && !files.count) [status addObject:@"当前没有匹配的日志或回放。请先完成一局再扫描；游戏可能没有保存可用日志。"];
            [status addObject:@"文件默认不选中。单个文件上限 20 MB，总导出上限 64 MB；也可以只导出诊断清单。"];
            if (files.count) [status addObject:@"要补齐配置、补丁和其他现有日志，请点底部“完整选择”再导出。"];
            for (NSString *error in errors) [status addObject:error];
            for (NSDictionary *scan in scans) {
                if ([scan[@"truncated"] boolValue]) [status addObject:@"部分目录达到扫描数量上限，清单可能不完整。"];
                for (NSString *error in scan[@"errors"]) [status addObject:error];
            }
            self.statusLines=status; [self setWorking:NO]; [self.tableView reloadData];
        });
    });
}
- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView { return 2; }
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section { return section==0?self.statusLines.count:self.files.count; }
- (NSString *)tableView:(UITableView *)tableView titleForHeaderInSection:(NSInteger)section { return section==0?@"扫描结果":@"选择需要导出的文件"; }
- (NSString *)tableView:(UITableView *)tableView titleForFooterInSection:(NSInteger)section {
    if (section!=1 || !self.report) return nil;
    unsigned long long bytes=0;
    for (NSUInteger i=self.selected.firstIndex;i!=NSNotFound;i=[self.selected indexGreaterThanIndex:i]) bytes+=[self.files[i][@"size"] unsignedLongLongValue];
    return [NSString stringWithFormat:@"已选 %lu/%lu 项，约 %@。完整选择会优先保留配置、补丁和主日志；实际未导出的项目会记录在清单中。",(unsigned long)self.selected.count,(unsigned long)self.files.count,[NSByteCountFormatter stringFromByteCount:(long long)bytes countStyle:NSByteCountFormatterCountStyleFile]];
}
- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    UITableViewCell *cell=[[UITableViewCell alloc] initWithStyle:UITableViewCellStyleSubtitle reuseIdentifier:nil];
    cell.textLabel.numberOfLines=0; cell.detailTextLabel.numberOfLines=0;
    if (indexPath.section==0) { cell.textLabel.text=self.statusLines[indexPath.row]; cell.selectionStyle=UITableViewCellSelectionStyleNone; return cell; }
    NSDictionary *file=self.files[indexPath.row];
    cell.textLabel.text=file[@"relative"];
    NSDate *date=[NSDate dateWithTimeIntervalSince1970:[file[@"modified"] doubleValue]];
    NSString *time=[NSDateFormatter localizedStringFromDate:date dateStyle:NSDateFormatterShortStyle timeStyle:NSDateFormatterShortStyle];
    NSString *size=[NSByteCountFormatter stringFromByteCount:[file[@"size"] longLongValue] countStyle:NSByteCountFormatterCountStyleFile];
    cell.detailTextLabel.text=[NSString stringWithFormat:@"%@ · %@ · 容器 %@",size,time,[file[@"container"] lastPathComponent]];
    cell.accessoryType=[self.selected containsIndex:indexPath.row]?UITableViewCellAccessoryCheckmark:UITableViewCellAccessoryNone;
    return cell;
}
- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    [tableView deselectRowAtIndexPath:indexPath animated:YES];
    if (self.busy || indexPath.section!=1) return;
    if ([self.selected containsIndex:indexPath.row]) [self.selected removeIndex:indexPath.row];
    else {
        if ([self.files[indexPath.row][@"size"] unsignedLongLongValue]>20*1024*1024) { [self showMessage:@"此文件超过 20 MB，请选择其他文件。清单仍会记录它的名称和大小。"]; return; }
        [self.selected addIndex:indexPath.row];
    }
    [tableView reloadRowsAtIndexPaths:@[indexPath] withRowAnimation:UITableViewRowAnimationNone];
    self.selectionMode=@"manual"; self.selectionIssues=@[]; [self refreshSelection];
}
- (void)exportFiles {
    if (self.busy || !self.report) return;
    [self setWorking:YES];
    NSIndexSet *selection=self.selected.copy; NSDictionary *baseReport=self.report; NSArray *inventory=self.files;
    NSString *selectionMode=self.selectionMode.copy; NSArray *selectionIssues=self.selectionIssues.copy;
    dispatch_async(dispatch_get_global_queue(QOS_CLASS_USER_INITIATED,0), ^{
        NSMutableDictionary<NSString *,NSData *> *entries=[NSMutableDictionary dictionary];
        NSMutableArray *copied=[NSMutableArray array], *skipped=[NSMutableArray array];
        __block NSUInteger total=0;
        [selection enumerateIndexesUsingBlock:^(NSUInteger index, BOOL *stop) {
            NSDictionary *file=inventory[index]; NSString *container=file[@"container"], *path=file[@"path"];
            NSString *resolved=path.stringByResolvingSymlinksInPath;
            NSString *prefix=[container.stringByResolvingSymlinksInPath stringByAppendingString:@"/"];
            NSDictionary *metadata=[NSDictionary dictionaryWithContentsOfFile:[container stringByAppendingPathComponent:@".com.apple.mobile_container_manager.metadata.plist"]];
            NSDictionary *attrs=[NSFileManager.defaultManager attributesOfItemAtPath:path error:nil];
            unsigned long long bytes=[attrs[NSFileSize] unsignedLongLongValue];
            if (!MJMatchesBundle(metadata[@"MCMMetadataIdentifier"]) || ![resolved hasPrefix:prefix] || ![attrs[NSFileType] isEqual:NSFileTypeRegular] || bytes>20*1024*1024 || total+bytes>63*1024*1024) { [skipped addObject:@{@"file":file[@"relative"],@"reason":@"文件不可读、路径变化，或超过导出限制"}]; return; }
            NSError *readError=nil;
            NSData *data=[NSData dataWithContentsOfFile:path options:0 error:&readError];
            if (!data || data.length>20*1024*1024 || total+data.length>63*1024*1024) { [skipped addObject:@{@"file":file[@"relative"],@"reason":readError.localizedDescription ?: @"文件读取时变化或超过导出限制"}]; return; }
            NSString *entry=[NSString stringWithFormat:@"files/%@/%@",container.lastPathComponent,file[@"relative"]];
            entries[entry]=data; total+=data.length; [copied addObject:entry];
        }];
        NSMutableDictionary *manifest=baseReport.mutableCopy; manifest[@"exported_files"]=copied; manifest[@"skipped_files"]=skipped;
        NSDictionary *coverage=MJExportCoverage(baseReport,inventory,selection,copied,skipped);
        manifest[@"selection_mode"]=selectionMode; manifest[@"selection_plan_skips"]=selectionIssues; manifest[@"coverage"]=coverage;
        NSError *error=nil;
        NSData *json=[NSJSONSerialization dataWithJSONObject:manifest options:NSJSONWritingPrettyPrinted error:&error];
        if (json) entries[@"manifest.json"]=json;
        NSData *zip=json?MJCreateZip(entries,&error):nil;
        NSURL *result=nil;
        if (zip) {
            NSString *directory=[NSHomeDirectory() stringByAppendingPathComponent:@"Documents/Exports"];
            if ([NSFileManager.defaultManager createDirectoryAtPath:directory withIntermediateDirectories:YES attributes:nil error:&error]) {
                NSString *name=[NSString stringWithFormat:@"MJLogs-%@-%.0f.zip",coverage[@"status"],NSDate.date.timeIntervalSince1970];
                NSURL *url=[NSURL fileURLWithPath:[directory stringByAppendingPathComponent:name]];
                if ([zip writeToURL:url options:NSDataWritingAtomic error:&error]) result=url;
            }
        }
        dispatch_async(dispatch_get_main_queue(), ^{
            [self setWorking:NO];
            if (!result) { [self showMessage:error.localizedDescription ?: @"导出失败"]; return; }
            self.statusLines=@[[NSString stringWithFormat:@"已导出 %lu/%lu 个候选文件。",(unsigned long)copied.count,(unsigned long)inventory.count],
                [NSString stringWithFormat:@"未选择 %@ 项；读取或大小限制跳过 %@ 项。",coverage[@"unselected_count"],coverage[@"skipped_count"]],
                [coverage[@"status"] isEqual:@"complete"]?@"本次扫描发现的候选文件已完整导出。":@"本次为部分导出或仅清单；完整性与原因已写入 manifest.json。"];
            [self.tableView reloadData];
            UIActivityViewController *share=[[UIActivityViewController alloc] initWithActivityItems:@[result] applicationActivities:nil];
            share.popoverPresentationController.barButtonItem=self.exportButton;
            [self presentViewController:share animated:YES completion:nil];
        });
    });
}
@end

@interface MJAppDelegate : UIResponder <UIApplicationDelegate>
@property(nonatomic,strong) UIWindow *window;
@end
@implementation MJAppDelegate
- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)options {
    self.window=[[UIWindow alloc] initWithFrame:UIScreen.mainScreen.bounds];
    MJReaderController *reader=[[MJReaderController alloc] initWithStyle:UITableViewStyleInsetGrouped];
    self.window.rootViewController=[[UINavigationController alloc] initWithRootViewController:reader];
    [self.window makeKeyAndVisible]; return YES;
}
@end
int main(int argc,char *argv[]) { @autoreleasepool { return UIApplicationMain(argc,argv,nil,NSStringFromClass(MJAppDelegate.class)); } }
