"""Runner-only cross-checks. Emit comparisons and sentinel counts, never hand values."""
import collections,json,pathlib,re

def inspect(directory):
    packets={};models={};wire=collections.defaultdict(list)
    for path in pathlib.Path(directory).iterdir():
        match=re.fullmatch(r'entry-(\d+)-seq-(\d+)\.(snapshot\.json|runtime\.json|tdr|protobuf)',path.name)
        if not match:continue
        entry,sequence=int(match[1]),int(match[2]);kind=match[3]
        if kind!='runtime.json':wire[entry].append(sequence)
        if kind not in ('snapshot.json','runtime.json'):continue
        value=json.loads(path.read_bytes())
        if kind=='snapshot.json':packets[(entry,sequence)]=value
        elif value.get('status')=='ok':models[(entry,sequence)]=value
    comparisons=[];sentinels=collections.defaultdict(collections.Counter)
    for (entry,sequence),model in models.items():
        for seat in model.get('seats',[]):
            if seat.get('view_seat')==0:continue
            for name,values in seat.get('aux_fields',{}).items():
                if name not in ('curRedrawMjValue','LastMJ','m_tianHuTile','m_multiTileRedrawList','m_laiZhiList'):continue
                for value in values:
                    key='zero' if value==0 else 'negative_one_sentinel' if value==-1 else 'valid_tile_code' if 1<=value<=9 or 11<=value<=19 or 21<=value<=29 or 31<=value<=37 else 'other_invalid'
                    sentinels[name][key]+=1
    def hands(node,seat):
        found=[]
        if isinstance(node,dict):
            if node.get('nSeat')==seat and isinstance(node.get('szTiles'),list) and type(node.get('nHandTilesNum')) is int:found.append(node['szTiles'][:node['nHandTilesNum']])
            for value in node.values():found.extend(hands(value,seat))
        elif isinstance(node,list):
            for value in node:found.extend(hands(value,seat))
        return found
    for (entry,sequence),packet in sorted(packets.items()):
        if packet.get('iType') not in (1,8):continue
        next_wire=min((s for s in wire[entry] if s>sequence),default=513)
        candidates=[(s,m) for (e,s),m in models.items() if e==entry and sequence<s<next_wire]
        if not candidates:comparisons.append({'entry':entry,'packet_sequence':sequence,'packet_type':packet['iType'],'status':'no_model_before_next_wire_event'});continue
        model_seq,model=min(candidates,key=lambda pair:pair[0]);own=next((s for s in model['seats'] if s['view_seat']==0),None)
        if not own:continue
        if packet['iType']==1:
            body=packet.get('stValue',{}).get('stNotifyDeal',{});tiles=body.get('szTiles');count=body.get('nTilesNum');references=[tiles[:count]] if isinstance(tiles,list) and type(count) is int and 0<=count<=32 else []
        else:references=hands(packet,model['self_seat'])
        if len(references)!=1:comparisons.append({'entry':entry,'packet_sequence':sequence,'packet_type':packet['iType'],'status':'ambiguous_or_missing_self_reference'});continue
        comparisons.append({'entry':entry,'packet_sequence':sequence,'packet_type':packet['iType'],'model_sequence':model_seq,'status':'compared','reference_tiles':len(references[0]),'model_tiles':len(own['hand']),'multiset_equal':collections.Counter(references[0])==collections.Counter(own['hand'])})
    return {'self_hand_comparisons':comparisons,'opponent_auxiliary_value_categories':{k:dict(v) for k,v in sentinels.items()},'limits':['Comparisons use the first model sample before the next captured wire event; local actions may still change a hand in that window.','Only equality, counts and fixed sentinel categories are emitted; no card values, hashes, IDs or private paths.','An empty_or_unsupported cache status is not proof that registered module models were scanned.']}
if __name__=='__main__':pathlib.Path('report/device/runtime-private-checks.json').write_text(json.dumps(inspect('private-input/decoded-game-frames'),indent=2)+'\n')
