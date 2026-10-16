// Browser monotonic timeline; producer timestamps are retained for diagnostics.
export class Motion {
  constructor(delay=100){this.delay=delay;this.reset();}
  reset(){this.samples=[];this.lastKey=null;this.lastSeen=-Infinity;}
  push(feed,now){
    const p=feed.position;
    if(!p||![p.x,p.y,p.z,feed.yaw].every(Number.isFinite))return false;
    const key=`${feed.session??''}:${feed.sequence}`;
    if(key===this.lastKey)return false;
    const last=this.samples.at(-1);
    if(last&&feed.session===last.session&&feed.sequence<last.sequence&&feed.timestamp<=last.timestamp)return false;
    const snap=!last||now-this.lastSeen>1000||feed.session!==last.session||feed.world!==last.world||feed.pawn!==last.pawn||Math.hypot(p.x-last.position.x,p.y-last.position.y,p.z-last.position.z)>10000;
    if(snap)this.samples=[];
    // Preserve producer spacing despite HTTP jitter; slowly track clock offset.
    let t=now;
    if(!snap&&Number.isFinite(feed.sampleTimeMs)&&Number.isFinite(last.sampleTimeMs)){
      const delta=feed.sampleTimeMs-last.sampleTimeMs;
      if(delta>0&&delta<1000)t=Math.min(now,Math.max(last.t+.01,last.t+delta+(now-last.t-delta)*.05));
    }
    this.samples.push({...feed,position:{...p},t});
    if(this.samples.length>64)this.samples.shift();
    this.lastSeen=now;this.lastKey=key;return snap;
  }
  at(now){
    if(!this.samples.length||now-this.lastSeen>1500)return null;
    const target=now-this.delay,s=this.samples;
    while(s.length>2&&s[1].t<=target)s.shift();
    const a=s[0],b=s[1]??a;
    const u=b.t===a.t?1:Math.max(0,Math.min(1,(target-a.t)/(b.t-a.t)));
    const delta=((b.yaw-a.yaw+540)%360)-180;
    return {position:{x:a.position.x+(b.position.x-a.position.x)*u,y:a.position.y+(b.position.y-a.position.y)*u,z:a.position.z+(b.position.z-a.position.z)*u},yaw:a.yaw+delta*u};
  }
}
