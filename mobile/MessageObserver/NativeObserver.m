#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <dispatch/dispatch.h>
#import <dlfcn.h>
#import <mach-o/dyld.h>
#import <stdlib.h>
#import <string.h>
#import <stdbool.h>
#import "../RuntimeProbe/ProbeStore.h"
#import "ObserverFiles.h"
#import "ManagedPayload.h"

typedef void *(*FDomain)(void);
typedef void *(*FCurrent)(void);
typedef void *(*FAttach)(void *);
typedef void (*FDetach)(void *);
typedef const void **(*FAssemblies)(void *,size_t *);
typedef const void *(*FImage)(const void *);
typedef const void *(*FCorlib)(void);
typedef void *(*FClass)(const void *,const char *,const char *);
typedef const void *(*FMethods)(void *,void **);
typedef const char *(*FName)(const void *);
typedef uint32_t (*FCount)(const void *);
typedef const void *(*FParam)(const void *,uint32_t);
typedef void *(*FClassType)(const void *);
typedef void *(*FArrayClass)(void *,uint32_t);
typedef const void *(*FMethod)(void *,const char *,int);
typedef void *(*FString)(const char *);
typedef void *(*FInvoke)(const void *,void *,void **,void **);
typedef void *(*FUnbox)(void *);
typedef uint32_t (*FRoot)(void *,bool);
typedef void (*FFreeRoot)(uint32_t);
typedef int32_t (*FStringLength)(void *);
typedef const uint16_t *(*FStringChars)(void *);
typedef struct {
    FDomain domain; FCurrent current; FAttach attach; FDetach detach; FAssemblies assemblies;
    FImage image; FCorlib corlib; FClass klass; FMethods methods; FName name; FCount count;
    FParam param; FClassType classType; FArrayClass arrayClass; FMethod method;
    FString string; FInvoke invoke; FUnbox unbox; FRoot root; FFreeRoot freeRoot; FStringLength stringLength; FStringChars stringChars;
} Runtime;
static NSString *ManagedPath,*EventsPath,*StatusPath;
static BOOL PayloadLoaded=NO,ObserverStarted=NO;
static uint32_t AssemblyRoot=0;
static UILabel *Badge;
static dispatch_source_t PollTimer;
static UILabel *ModelPanel;
static BOOL ModelReadErrorReported=NO;
static void ReadModelPanel(void);
static void ShowModelPanel(NSString *message){
    @try{
        UIWindow *window=nil;
        for(UIWindow *candidate in UIApplication.sharedApplication.windows)if(candidate.isKeyWindow && !candidate.hidden){window=candidate;break;}
        if(!window)return;
        if(!ModelPanel){ModelPanel=[[UILabel alloc] initWithFrame:CGRectZero];ModelPanel.userInteractionEnabled=NO;ModelPanel.numberOfLines=4;ModelPanel.font=[UIFont systemFontOfSize:11 weight:UIFontWeightMedium];ModelPanel.textColor=UIColor.whiteColor;ModelPanel.backgroundColor=[UIColor colorWithWhite:0 alpha:0.78];ModelPanel.layer.cornerRadius=6;ModelPanel.clipsToBounds=YES;ModelPanel.adjustsFontSizeToFitWidth=YES;ModelPanel.minimumScaleFactor=0.65;ModelPanel.accessibilityIdentifier=@"mj-runtime-model-panel";}
        if(ModelPanel.superview!=window){[ModelPanel removeFromSuperview];[window addSubview:ModelPanel];}
        CGFloat top=MAX(8,window.safeAreaInsets.top+2)+32;
        ModelPanel.frame=CGRectMake(8,top,MIN(560,window.bounds.size.width-16),88);
        ModelPanel.text=message;[window bringSubviewToFront:ModelPanel];
    }@catch(NSException *exception){}
}
static void ShowBadge(NSString *message,BOOL failure){
    @try{
        UIWindow *window=nil;
        for(UIWindow *candidate in UIApplication.sharedApplication.windows)if(candidate.isKeyWindow && !candidate.hidden){window=candidate;break;}
        if(!window)return;
        if(!Badge){Badge=[[UILabel alloc] initWithFrame:CGRectZero];Badge.userInteractionEnabled=NO;Badge.font=[UIFont systemFontOfSize:12 weight:UIFontWeightMedium];Badge.textAlignment=NSTextAlignmentCenter;Badge.textColor=UIColor.whiteColor;Badge.layer.cornerRadius=6;Badge.clipsToBounds=YES;Badge.accessibilityIdentifier=@"mj-message-observer-status";}
        if(Badge.superview!=window){[Badge removeFromSuperview];[window addSubview:Badge];}
        Badge.frame=CGRectMake(8,MAX(8,window.safeAreaInsets.top+2),MIN(300,window.bounds.size.width-16),28);
        Badge.backgroundColor=failure?[UIColor colorWithRed:0.65 green:0.12 blue:0.12 alpha:0.85]:[UIColor colorWithWhite:0 alpha:0.7];
        Badge.text=message;[window bringSubviewToFront:Badge];
    }@catch(NSException *exception){}
}
static void PollManagedStatus(void){
    if(!StatusPath)return;
    NSData *data=[NSData dataWithContentsOfFile:StatusPath];if(!data || data.length>64*1024)return;
    NSString *text=[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    for(NSString *line in [[text componentsSeparatedByString:@"\n"] reverseObjectEnumerator]){
        NSData *bytes=[line dataUsingEncoding:NSUTF8StringEncoding];if(!bytes)continue;
        id object=[NSJSONSerialization JSONObjectWithData:bytes options:0 error:NULL];
        if(![object isKindOfClass:NSDictionary.class] || ![object[@"kind"] isEqual:@"managed_status"])continue;
        NSString *code=object[@"status"];id number=object[@"value"];
        NSInteger count=[number isKindOfClass:NSNumber.class]?[number integerValue]:-1;
        if(([code isEqual:@"first_event_recorded"] || [code isEqual:@"capture_progress"]) && count>=1 && count<=512)ShowBadge([NSString stringWithFormat:@"诊断：已保存 ≥%ld 条记录",(long)count],NO);
        else if([code isEqual:@"observer_started"])ShowBadge(@"诊断：已就绪，可以进入对局",NO);
        else if([code isEqual:@"capture_limit_reached"])ShowBadge(@"诊断：记录已到上限，请导出",NO);
        else if([code isEqual:@"capture_failed"] || [code isEqual:@"unexpected_payload"])ShowBadge(@"诊断：记录待检查，请导出日志",YES);
        else continue;
        break;
    }
}
static void StartPolling(void){
    if(PollTimer)return;
    PollTimer=dispatch_source_create(DISPATCH_SOURCE_TYPE_TIMER,0,0,dispatch_get_main_queue());
    dispatch_source_set_timer(PollTimer,dispatch_time(DISPATCH_TIME_NOW,NSEC_PER_SEC),2*NSEC_PER_SEC,NSEC_PER_SEC/4);
    dispatch_source_set_event_handler(PollTimer,^{@try{if(UIApplication.sharedApplication.applicationState==UIApplicationStateActive){PollManagedStatus();ReadModelPanel();}} @catch(NSException *exception){ShowModelPanel(@"客户端模型：读取未完成，请导出记录");}});
    dispatch_resume(PollTimer);
}
static void Status(NSString *code,NSUInteger attempt) {
    NSString *documents=[NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,NSUserDomainMask,YES) firstObject];
    if(documents)MJProbeAppendRecord(documents,@{@"kind":@"native_status",@"schema":@1,@"version":@"0.3.0",@"status":code,@"attempt":@(attempt),@"subscription_registered":@(ObserverStarted)},NULL);
    if([code isEqual:@"observer_started"]){ShowBadge(@"诊断：已就绪，可以进入对局",NO);StartPolling();}
    else if([code isEqual:@"library_loaded"] || [code isEqual:@"preparing_files"] || [code isEqual:@"managed_assembly_loaded"])ShowBadge(@"诊断：正在启动消息观察器",NO);
    else if(attempt>=3 || [code isEqual:@"unsupported_game_build"] || [code isEqual:@"diagnostic_files_failed"])ShowBadge(@"诊断：启动未完成，请导出日志",YES);
    else ShowBadge(@"诊断：等待消息入口就绪",NO);
}
static void *Find(void *handle,const char *name){void *p=dlsym(RTLD_DEFAULT,name);return p? p:(handle?dlsym(handle,name):NULL);}
static void *UnityHandle(void){
    uint32_t n=_dyld_image_count();
    for(uint32_t i=0;i<n;i++){const char *p=_dyld_get_image_name(i);if(p && [[NSString stringWithUTF8String:p] hasSuffix:@"/UnityFramework.framework/UnityFramework"])return dlopen(p,RTLD_LAZY|RTLD_NOLOAD);}
    return NULL;
}
static BOOL Bind(Runtime *r,void *handle){
#define BIND(member,symbol,Type) r->member=(Type)Find(handle,symbol);if(!r->member)return NO
    BIND(domain,"il2cpp_domain_get",FDomain);BIND(current,"il2cpp_thread_current",FCurrent);
    BIND(attach,"il2cpp_thread_attach",FAttach);BIND(detach,"il2cpp_thread_detach",FDetach);
    BIND(assemblies,"il2cpp_domain_get_assemblies",FAssemblies);BIND(image,"il2cpp_assembly_get_image",FImage);
    BIND(corlib,"il2cpp_get_corlib",FCorlib);BIND(klass,"il2cpp_class_from_name",FClass);
    BIND(methods,"il2cpp_class_get_methods",FMethods);BIND(name,"il2cpp_method_get_name",FName);
    BIND(count,"il2cpp_method_get_param_count",FCount);BIND(param,"il2cpp_method_get_param",FParam);
    BIND(classType,"il2cpp_class_from_type",FClassType);BIND(arrayClass,"il2cpp_array_class_get",FArrayClass);
    BIND(method,"il2cpp_class_get_method_from_name",FMethod);BIND(string,"il2cpp_string_new",FString);
    BIND(invoke,"il2cpp_runtime_invoke",FInvoke);BIND(unbox,"il2cpp_object_unbox",FUnbox);
    BIND(root,"il2cpp_gchandle_new",FRoot);BIND(freeRoot,"il2cpp_gchandle_free",FFreeRoot);
    r->stringLength=(FStringLength)Find(handle,"il2cpp_string_length");
    r->stringChars=(FStringChars)Find(handle,"il2cpp_string_chars");
#undef BIND
    return YES;
}
static void *FindClass(Runtime *r,void *domain,const char *space,const char *name){
    size_t total=0;const void **all=r->assemblies(domain,&total);size_t count=total>512?512:total;
    if(!all || !count)return NULL;
    const void **copy=calloc(count,sizeof(void *));if(!copy)return NULL;memcpy(copy,all,count*sizeof(void *));
    void *result=NULL;
    for(size_t i=0;i<count;i++){if(!copy[i])continue;const void *image=r->image(copy[i]);if(image && (result=r->klass(image,space,name)))break;}
    free(copy);return result;
}
static void ReadModelPanel(void){
    if(!ObserverStarted)return;
    void *handle=UnityHandle();Runtime r={0};
    if(!Bind(&r,handle) || !r.stringLength || !r.stringChars){ShowModelPanel(@"客户端模型：运行时接口不可用");if(handle)dlclose(handle);return;}
    void *domain=r.domain();if(!domain){ShowModelPanel(@"客户端模型：等待运行时");if(handle)dlclose(handle);return;}
    void *previous=r.current(),*thread=previous?previous:r.attach(domain);
    if(!thread){ShowModelPanel(@"客户端模型：读取未完成");if(handle)dlclose(handle);return;}
    void *klass=FindClass(&r,domain,"MJDiagnostic","Observer");const void *method=klass?r.method(klass,"ReadModel",0):NULL;
    void *exception=NULL,*result=method?r.invoke(method,NULL,NULL,&exception):NULL;
    uint32_t root=result?r.root(result,false):0;
    NSString *text=nil;
    if(result && root && !exception){
        int32_t length=r.stringLength(result);const uint16_t *chars=r.stringChars(result);
        if(chars && length>0 && length<=8192)text=[[NSString alloc] initWithCharacters:(const unichar *)chars length:(NSUInteger)length];
    }
    if(root)r.freeRoot(root);
    if(!previous)r.detach(thread);if(handle)dlclose(handle);
    NSData *bytes=[text dataUsingEncoding:NSUTF8StringEncoding];
    id snapshot=bytes?[NSJSONSerialization JSONObjectWithData:bytes options:0 error:NULL]:nil;
    NSString *panel=[snapshot isKindOfClass:NSDictionary.class]?snapshot[@"panel_text"]:nil;
    if([snapshot isKindOfClass:NSDictionary.class] && [snapshot[@"kind"] isEqual:@"runtime_model"] && [panel isKindOfClass:NSString.class] && panel.length>0 && panel.length<=4096){ShowModelPanel(panel);return;}
    ShowModelPanel(@"客户端模型：读取未完成，请导出记录");
    if(!ModelReadErrorReported){
        ModelReadErrorReported=YES;
        NSString *documents=[NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,NSUserDomainMask,YES) firstObject];
        if(documents)MJProbeAppendRecord(documents,@{@"kind":@"native_status",@"schema":@1,@"version":@"0.3.0",@"status":@"model_reader_invoke_failed",@"attempt":@0,@"subscription_registered":@(ObserverStarted)},NULL);
    }
}
static void Bootstrap(NSUInteger attempt){
    @autoreleasepool{
        if(ObserverStarted || !MJProbeMatchesBundle(NSBundle.mainBundle.bundleIdentifier))return;
        void *handle=UnityHandle();Runtime r={0};
        if(!Bind(&r,handle)){Status(@"required_api_unavailable",attempt);if(handle)dlclose(handle);return;}
        void *domain=r.domain();if(!domain){Status(@"domain_not_ready",attempt);if(handle)dlclose(handle);return;}
        void *previous=r.current(),*thread=previous?previous:r.attach(domain);
        if(!thread){Status(@"thread_attach_failed",attempt);if(handle)dlclose(handle);return;}
        uint32_t roots[8]={0};size_t rootsUsed=0;
#define ROOT(value) do{if(value && rootsUsed<8)roots[rootsUsed++]=r.root(value,false);}while(0)
        do{
            if(!FindClass(&r,domain,"HappyMahjong","TSDK4CSharpService") || !FindClass(&r,domain,"strange.extensions.dispatcher.eventdispatcher.api","EventCallback")){Status(@"game_interfaces_not_ready",attempt);break;}
            const void *core=r.corlib();if(!core){Status(@"corlib_not_ready",attempt);break;}
            if(!PayloadLoaded){
                void *fileClass=r.klass(core,"System.IO","File");void *assemblyClass=r.klass(core,"System.Reflection","Assembly");
                void *byteClass=r.klass(core,"System","Byte");
                if(!fileClass || !assemblyClass || !byteClass){Status(@"loader_types_unavailable",attempt);break;}
                void *byteArrayClass=r.arrayClass(byteClass,1);
                const void *read=r.method(fileClass,"ReadAllBytes",1),*load=NULL,*method=NULL;void *iterator=NULL;NSUInteger scanned=0;
                while(scanned++<256 && (method=r.methods(assemblyClass,&iterator))){
                    const char *name=r.name(method);
                    if(name && strcmp(name,"Load")==0 && r.count(method)==1 && r.classType(r.param(method,0))==byteArrayClass){load=method;break;}
                }
                if(!read || !load || !byteArrayClass){Status(@"byte_array_loader_unavailable",attempt);break;}
                void *path=r.string(ManagedPath.UTF8String);ROOT(path);void *args[]={path};void *exception=NULL;
                void *bytes=r.invoke(read,NULL,args,&exception);
                if(exception || !bytes){Status(@"managed_payload_read_failed",attempt);break;}ROOT(bytes);
                void *loadArgs[]={bytes};exception=NULL;void *assembly=r.invoke(load,NULL,loadArgs,&exception);
                if(exception || !assembly){Status(@"managed_assembly_load_failed",attempt);break;}
                AssemblyRoot=r.root(assembly,false);PayloadLoaded=YES;Status(@"managed_assembly_loaded",attempt);
            }
            void *observer=FindClass(&r,domain,"MJDiagnostic","Observer");
            const void *start=observer?r.method(observer,"Start",2):NULL;
            if(!start){Status(@"observer_entry_not_visible",attempt);break;}
            void *events=r.string(EventsPath.UTF8String),*status=r.string(StatusPath.UTF8String);ROOT(events);ROOT(status);
            void *args[]={events,status};void *exception=NULL;void *result=r.invoke(start,NULL,args,&exception);
            if(exception || !result){Status(@"observer_start_failed",attempt);break;}
            uint8_t *value=r.unbox(result);
            ObserverStarted=value && *value;
            Status(ObserverStarted?@"observer_started":@"observer_not_ready",attempt);
        }while(0);
        for(size_t i=0;i<rootsUsed;i++)if(roots[i])r.freeRoot(roots[i]);
        if(!previous)r.detach(thread);if(handle)dlclose(handle);
#undef ROOT
    }
}
__attribute__((constructor)) static void MJMessageObserverStart(void){
    @autoreleasepool{
        if(!MJProbeMatchesBundle(NSBundle.mainBundle.bundleIdentifier))return;
        dispatch_async(dispatch_get_main_queue(),^{
            @try{
                Status(@"library_loaded",0);
                id build=[NSBundle.mainBundle objectForInfoDictionaryKey:@"CFBundleVersion"];
                if(![build isKindOfClass:NSString.class] || ![build isEqualToString:@"8.19.112"]){Status(@"unsupported_game_build",0);return;}
                NSString *documents=[NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,NSUserDomainMask,YES) firstObject];
                NSData *assembly=[NSData dataWithBytes:MJManagedPayload length:MJManagedPayloadLength];
                NSString *managed=nil,*events=nil,*status=nil;
                if(!MJObserverPrepareFiles(documents,assembly,&managed,&events,&status,NULL)){Status(@"diagnostic_files_failed",0);return;}
                ManagedPath=managed;EventsPath=events;StatusPath=status;
                const NSUInteger delays[]={10,30,60};
                for(NSUInteger i=0;i<3;i++){
                    NSUInteger attempt=i+1;
                    dispatch_after(dispatch_time(DISPATCH_TIME_NOW,(int64_t)(delays[i]*NSEC_PER_SEC)),dispatch_get_main_queue(),^{
                        @try{Bootstrap(attempt);}@catch(NSException *exception){Status(@"objective_c_error",attempt);}
                    });
                }
            }@catch(NSException *exception){Status(@"objective_c_error",0);}
        });
    }
}
