"""Publish only counters from privately decoded protocol objects, never tile values."""
import collections,json,pathlib,re,zlib

def proto_fields(raw,_visit=None):
    index=0;counts=collections.Counter()
    def varint():
        nonlocal index
        value=0
        for shift in range(0,70,7):
            if index>=len(raw):raise ValueError()
            byte=raw[index];index+=1;value|=(byte&127)<<shift
            if not byte&128:return value
        raise ValueError()
    while index<len(raw):
        key=varint();tag,wire=key>>3,key&7;child=None
        if not 0<tag<536870912:raise ValueError()
        counts[str(tag)]+=1
        if sum(counts.values())>100000:raise ValueError()
        if wire==0:varint()
        elif wire==1:index+=8
        elif wire==2:
            length=varint();child=raw[index:index+length];index+=length
        elif wire==5:index+=4
        else:raise ValueError()
        if index>len(raw):raise ValueError()
        if _visit is not None:_visit(tag,wire,child)
    return dict(counts)

def advice_schema(raw):
    scalar=(None,0)
    schemas={'unit':{1:('advice',2)},'advice':{1:scalar,2:scalar,3:('ting',2)},'ting':{1:('discard',2)},'discard':{1:scalar,2:('tile',2),3:scalar},'tile':{1:scalar,2:scalar,3:scalar,4:scalar,5:('fan',2),6:scalar,7:scalar},'fan':{i:scalar for i in range(1,6)}}
    counts=collections.Counter();unknown=0;mismatch=0
    def walk(data,name):
        nonlocal unknown,mismatch
        counts[name]+=1
        if sum(counts.values())>100000:raise ValueError()
        def visit(tag,wire,child):
            nonlocal unknown,mismatch
            expected=schemas[name].get(tag)
            if expected is None:unknown+=1;return
            if wire!=expected[1]:mismatch+=1;return
            if expected[0] is not None:walk(child,expected[0])
        proto_fields(data,visit)
    try:walk(raw,'unit');valid=True
    except ValueError:valid=False
    return {'parsed':valid,'message_counts':dict(counts),'unknown_field_count':unknown,'wire_mismatch_count':mismatch}

def lz4_block(raw,expected):
    if not 0<expected<=4*1024*1024:raise ValueError()
    index=0;output=bytearray()
    def extension(length):
        nonlocal index
        if length==15:
            while True:
                if index>=len(raw):raise ValueError()
                byte=raw[index];index+=1;length+=byte
                if length>expected:raise ValueError()
                if byte!=255:break
        return length
    while index<len(raw):
        token=raw[index];index+=1;literal=extension(token>>4)
        if index+literal>len(raw) or len(output)+literal>expected:raise ValueError()
        output.extend(raw[index:index+literal]);index+=literal
        if index==len(raw):break
        if index+2>len(raw):raise ValueError()
        offset=raw[index]|raw[index+1]<<8;index+=2
        if offset==0 or offset>len(output):raise ValueError()
        match=extension(token&15)+4
        if len(output)+match>expected:raise ValueError()
        for unused in range(match):output.append(output[-offset])
    if len(output)!=expected:raise ValueError()
    return bytes(output)

def embedded(value):
    count,original,cards=(value.get(k) for k in ('nGameBuf','iOriginSize','szGameBuf'))
    if type(count) is not int or not 0<count<=32767 or type(original) is not int or not 0<original<=4*1024*1024 or not isinstance(cards,list) or len(cards)<count or not all(type(c) is int and -128<=c<=255 for c in cards):return {'status':'invalid_buffer_shape'}
    raw=bytes(c&255 for c in cards[:count])
    for window in (15,-15,31):
        try:
            stream=zlib.decompressobj(window);clear=stream.decompress(raw,4*1024*1024+1)
            if len(clear)!=original or not stream.eof or stream.unconsumed_tail or stream.unused_data:continue
            tags=proto_fields(clear)
            return {'status':'decompressed_proto_fields','compression':{15:'zlib',-15:'raw_deflate',31:'gzip'}[window],'compressed_bytes':count,'decoded_bytes':len(clear),'top_level_field_counts':tags,'only_known_advice_field':set(tags)=={'1'},'advice_schema':advice_schema(clear)}
        except (ValueError,zlib.error):continue
    try:
        clear=lz4_block(raw,original);tags=proto_fields(clear)
        return {'status':'decompressed_proto_fields','compression':'lz4_block','compressed_bytes':count,'decoded_bytes':len(clear),'top_level_field_counts':tags,'only_known_advice_field':set(tags)=={'1'},'advice_schema':advice_schema(clear)}
    except ValueError:pass
    return {'status':'not_decoded','compressed_bytes':count,'reported_origin_bytes':original}

def lua_shape(value):
    size=value.get('nLuaBuffLen');data=value.get('szLuaBuff')
    if type(size) is not int or not 0<=size<=65536 or not isinstance(data,list) or len(data)<size or not all(type(c) is int and -128<=c<=255 for c in data):return {'status':'invalid_shape'}
    raw=bytes(c&255 for c in data[:size]).rstrip(b'\0');signals=collections.Counter();arrays=[];scalar_kinds=collections.Counter()
    try:parsed=json.loads(raw)
    except (ValueError,UnicodeError,RecursionError):
        try:text=raw.decode('utf8')
        except UnicodeError:return {'status':'not_utf8_json','bytes':size}
        conditions=re.findall(r'condition_(\d{1,2})',text)
        known=bool(conditions) and all(0<=int(c)<50 for c in conditions) and re.fullmatch(r'(?:\s|return|condition_\d{1,2}|and|or|[()])+',text) is not None
        return {'status':'known_rule_condition_expression' if known else 'unclassified_utf8','bytes':size,'rule_type_one':value.get('iLuaType')==1,'condition_reference_count':len(conditions) if known else None}
    def walk(node):
        if isinstance(node,dict):
            for key,child in node.items():
                for signal in ('hand','tile','card','wall','seed','rand'):
                    if signal in key.lower():signals[signal]+=1
                walk(child)
        elif isinstance(node,list):
            if all(type(c) is int for c in node):arrays.append(len(node))
            for child in node:walk(child)
        else:scalar_kinds[type(node).__name__]+=1
    walk(parsed)
    return {'status':'json_shape','bytes':size,'root_fields':len(parsed) if isinstance(parsed,dict) else None,'gameplay_key_signals':dict(signals),'numeric_array_lengths':arrays,'scalar_type_counts':dict(scalar_kinds)}

def inspect(directory):
    frames=[]
    format_path=pathlib.Path(directory)/'capture-formats.json'
    formats=json.loads(format_path.read_text()) if format_path.exists() else {}
    for path in sorted(pathlib.Path(directory).glob('entry-*-seq-*.tdr.json')):
        match=re.fullmatch(r'entry-(\d+)-seq-(\d+)\.tdr\.json',path.name)
        if not match:continue
        data=json.loads(path.read_text());rows=[];waits=[];draws=[];buffers=[];configs=[];tlvs=collections.Counter()
        def walk(value):
            if isinstance(value,dict):
                if type(value.get('nType')) is int and 'nLen' in value:tlvs[str(value['nType'])]+=1
                if all(k in value for k in ('nSeat','nHandTilesNum','szTiles')) and type(value['nSeat']) is int and 0<=value['nSeat']<=3:
                    cards=value['szTiles'];count=value['nHandTilesNum']
                    if isinstance(cards,list) and type(count) is int and 0<=count<=32 and all(type(c) is int for c in cards):
                        used=cards[:count]
                        rows.append({'seat':value['nSeat'],'declared_tiles':count,'array_length':len(cards),'nonzero_tiles':sum(c!=0 for c in used),'zero_tiles':sum(c==0 for c in used),'length_agrees':len(cards)==count})
                if all(k in value for k in ('chChair','chExCmd','nExtraTilesNum')):
                    cards=value.get('ExtraTiles',[]);count=value['nExtraTilesNum'];seat=value['chChair']
                    if type(seat) is int and -1<=seat<=3 and type(count) is int and 0<=count<=4096 and isinstance(cards,list) and all(type(c) is int for c in cards):waits.append({'seat':seat,'extra_command_nonzero':value['chExCmd']!=0,'declared_extra_tiles':count,'array_length':len(cards),'nonzero_extra_tiles':sum(c!=0 for c in cards[:count])})
                if all(k in value for k in ('nSeatID','nDrawTileNum','chTile')) and type(value['nSeatID']) is int and 0<=value['nSeatID']<=3:
                    cards=value.get('DrawTiles',[]);count=value['nDrawTileNum']
                    if type(count) is int and 0<=count<=32 and isinstance(cards,list) and all(type(c) is int for c in cards):draws.append({'seat':value['nSeatID'],'single_tile_nonzero':value['chTile']!=0,'declared_draw_tiles':count,'array_length':len(cards),'nonzero_draw_tiles':sum(c!=0 for c in cards[:count])})
                if all(k in value for k in ('nGameBuf','iOriginSize','szGameBuf')):buffers.append(embedded(value))
                if all(k in value for k in ('nLuaBuffLen','szLuaBuff')):configs.append(lua_shape(value))
                for v in value.values():walk(v)
            elif isinstance(value,list):
                for v in value:walk(v)
        snapshot_details=None
        if formats.get(path.name)=='public_field_snapshot':
            inventory=[];seat_counts=[];recommendations=[]
            root_deal=data.get('stValue',{}).get('stNotifyDeal',{}) if data.get('iType')==1 else {}
            initial=root_deal.get('szTiles');initial_count=root_deal.get('nTilesNum')
            initial=initial[:initial_count] if isinstance(initial,list) and type(initial_count) is int and 0<=initial_count<=32 else None
            reference_kind='deal_self_array' if initial is not None else None
            if initial is None and data.get('iType')==8:
                references=set()
                def own_reference(node):
                    if isinstance(node,dict):
                        cards=node.get('szTiles');count=node.get('nHandTilesNum')
                        if node.get('nSeat')==0 and type(count) is int and 0<count<=32 and isinstance(cards,list) and len(cards)==count and all(type(c) is int for c in cards):references.add(tuple(cards))
                        for child in node.values():own_reference(child)
                    elif isinstance(node,list):
                        for child in node:own_reference(child)
                own_reference(data.get('stValue',{}).get('stTableInfo',{}))
                if len(references)==1:initial=list(next(iter(references)));initial_count=len(initial);reference_kind='table_seat_0_hand'
            def shape(value,field_path='',seat=None):
                if isinstance(value,dict):
                    explicit=value.get('nSeat',value.get('nSeatID'))
                    if type(explicit) is int and 0<=explicit<=3:seat=explicit
                    if type(value.get('nHandTilesNum')) is int and type(value.get('nSeat')) is int and 0<=value['nSeat']<=3:
                        seat_counts.append({'seat':value['nSeat'],'declared_hand_tiles':value['nHandTilesNum'],'has_tile_array':isinstance(value.get('szTiles'),list)})
                    candidate=value.get('CurHandTiles')
                    if isinstance(candidate,list) and initial is not None:recommendations.append(collections.Counter(candidate)==collections.Counter(initial))
                    for key,child in value.items():
                        if re.fullmatch(r'[A-Za-z_][A-Za-z0-9_]{0,79}',key):shape(child,field_path+'.'+key,seat)
                elif isinstance(value,list):
                    if value and all(type(v) is int for v in value):inventory.append({'path':field_path,'length':len(value),'nonzero_entries':sum(v!=0 for v in value),'zero_entries':sum(v==0 for v in value),'explicit_ancestor_seat':seat})
                    else:
                        for child in value:shape(child,field_path+'[*]',seat)
            shape(data)
            snapshot_details={'numeric_arrays':inventory,'seat_hand_counts':seat_counts}
            if initial is not None:
                check={'reference_kind':reference_kind,'reference_tiles':len(initial),'recommendation_hands_compared':len(recommendations),'recommendation_hands_matching':sum(recommendations)}
                snapshot_details['self_hand_reference']=check
                if root_deal:snapshot_details['deal_self_tiles']={'declared_tiles':initial_count,'array_length':len(root_deal['szTiles']),'nonzero_tiles':sum(v!=0 for v in initial),'recommendation_hands_compared':len(recommendations),'recommendation_hands_matching':sum(recommendations)}
        walk(data)
        frames.append({'entry':int(match[1]),'sequence':int(match[2]),'message_type':data.get('iType'),'source_encoding':'public_field_snapshot' if formats.get(path.name)=='public_field_snapshot' else 'reencoded_decoded_event','snapshot_details':snapshot_details,'hand_fields':rows,'wait_fields':waits,'draw_fields':draws,'embedded_proto_buffers':buffers,'lua_config_shapes':configs,'tlv_type_counts':dict(tlvs)})
    states={};rounds={}
    for frame in frames:
        entry=frame['entry'];phase=states.get(entry,'before_hu')
        if frame['message_type']==1:
            rounds[entry]=rounds.get(entry,0)+1;phase='before_hu';states[entry]=phase
        frame['captured_deal_number']=rounds.get(entry,0)
        if frame['message_type'] in (50,51,54):phase='after_hu_or_finish';states[entry]=phase
        frame['phase']=phase
    return {'decoded_frame_count':len(frames),'frames':frames,'limits':['Card values, identifiers, raw payloads and private paths are never emitted.','Only active union branches are traversed by the decoder. Hidden display is not established by message counts.','Seat identity must be reconciled independently; post-Hu hands do not establish live visibility.','Phase is relative to captured Hu/finish only; a missing earlier Hu cannot be inferred absent.']}
if __name__=='__main__':pathlib.Path('report/device/decoded-gameplay-counts.json').write_text(json.dumps(inspect('private-output'),indent=2)+'\n')
