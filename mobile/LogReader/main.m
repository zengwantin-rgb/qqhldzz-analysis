#import <UIKit/UIKit.h>
#import "Scanner.h"

@interface MJReaderController : UITableViewController
@property(nonatomic,strong) NSArray<NSDictionary *> *files;
@property(nonatomic,strong) NSMutableIndexSet *selected;
@property(nonatomic,strong) NSDictionary *report;
@property(nonatomic,strong) NSArray<NSString *> *statusLines;
@property(nonatomic,strong) UIBarButtonItem *exportButton;
@property(nonatomic,assign) BOOL busy;
@end

@implementation MJReaderController
- (void)viewDidLoad {
    [super viewDidLoad];
    self.title=@"麻将日志导出";
    self.files=@[]; self.selected=[NSMutableIndexSet indexSet];
    self.statusLines=@[@"先在大众房间打一局，再返回这里点击“扫描”。", @"选择日志或回放文件后，点击“导出”保存到“文件”或分享。", @"本工具读取现有文件；日志未必包含原始对局消息。"];
    self.navigationItem.leftBarButtonItem=[[UIBarButtonItem alloc] initWithTitle:@"扫描" style:UIBarButtonItemStylePlain target:self action:@selector(scan)];
    self.exportButton=[[UIBarButtonItem alloc] initWithTitle:@"导出" style:UIBarButtonItemStylePlain target:self action:@selector(exportFiles)];
    self.exportButton.enabled=NO; self.navigationItem.rightBarButtonItem=self.exportButton;
    self.tableView.estimatedRowHeight=72; self.tableView.rowHeight=UITableViewAutomaticDimension;
    self.tableView.accessibilityIdentifier=@"log-reader-table";
}
- (void)showMessage:(NSString *)message {
    UIAlertController *alert=[UIAlertController alertControllerWithTitle:@"麻将日志导出" message:message preferredStyle:UIAlertControllerStyleAlert];
    [alert addAction:[UIAlertAction actionWithTitle:@"好" style:UIAlertActionStyleDefault handler:nil]];
    [self presentViewController:alert animated:YES completion:nil];
}
- (void)setWorking:(BOOL)value {
    self.busy=value; self.navigationItem.leftBarButtonItem.enabled=!value;
    self.exportButton.enabled=!value && self.report!=nil;
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
        NSDictionary *report=@{@"schema":@1, @"tool_version":@"1.0", @"bundle_id":@"com.tencent.happymj", @"device_system":UIDevice.currentDevice.systemVersion,
            @"created":@([NSDate.date timeIntervalSince1970]), @"app_version":version, @"scans":scans, @"errors":errors,
            @"scope":@"Existing local files only. No process injection, memory capture, network capture, or server requests.",
            @"interpretation":@"A filename or replay file does not prove live opponents hands are available. Some logs are encrypted or lack message bodies."};
        dispatch_async(dispatch_get_main_queue(), ^{
            self.report=report; self.files=files; [self.selected removeAllIndexes];
            self.exportButton.title=@"导出(0)";
            NSMutableArray *status=[NSMutableArray arrayWithObjects:[NSString stringWithFormat:@"找到 %lu 个欢乐麻将容器、%lu 个候选文件。",(unsigned long)containers.count,(unsigned long)files.count],nil];
            if (version.count) [status addObject:[NSString stringWithFormat:@"游戏版本 %@（%@）",version[@"version"],version[@"build"]]];
            if (!containers.count) [status addObject:@"未找到原包名 com.tencent.happymj 的容器，或访问被系统拒绝。请导出清单以便检查；分身或改包名版本不会被自动扫描。"];
            if (containers.count && !files.count) [status addObject:@"当前没有匹配的日志或回放。请先完成一局再扫描；游戏可能没有保存可用日志。"];
            [status addObject:@"文件默认不选中。单个文件上限 20 MB，总导出上限 64 MB；也可以只导出诊断清单。"];
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
    self.exportButton.title=[NSString stringWithFormat:@"导出(%lu)",(unsigned long)self.selected.count];
}
- (void)exportFiles {
    if (self.busy || !self.report) return;
    [self setWorking:YES];
    NSIndexSet *selection=self.selected.copy; NSDictionary *baseReport=self.report; NSArray *inventory=self.files;
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
        NSError *error=nil;
        NSData *json=[NSJSONSerialization dataWithJSONObject:manifest options:NSJSONWritingPrettyPrinted error:&error];
        if (json) entries[@"manifest.json"]=json;
        NSData *zip=json?MJCreateZip(entries,&error):nil;
        NSURL *result=nil;
        if (zip) {
            NSString *directory=[NSHomeDirectory() stringByAppendingPathComponent:@"Documents/Exports"];
            if ([NSFileManager.defaultManager createDirectoryAtPath:directory withIntermediateDirectories:YES attributes:nil error:&error]) {
                NSString *name=[NSString stringWithFormat:@"MJLogs-%.0f.zip",NSDate.date.timeIntervalSince1970];
                NSURL *url=[NSURL fileURLWithPath:[directory stringByAppendingPathComponent:name]];
                if ([zip writeToURL:url options:NSDataWritingAtomic error:&error]) result=url;
            }
        }
        dispatch_async(dispatch_get_main_queue(), ^{
            [self setWorking:NO];
            if (!result) { [self showMessage:error.localizedDescription ?: @"导出失败"]; return; }
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
