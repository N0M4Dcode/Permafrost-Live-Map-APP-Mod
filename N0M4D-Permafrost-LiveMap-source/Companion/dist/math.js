export const finite = n => typeof n==='number' && Number.isFinite(n);
export function project(p,m){return {x:m[0]*p.x+m[1]*p.y+m[2],y:m[3]*p.x+m[4]*p.y+m[5]};}
export function solveAffine(points){
  if(points.length!==3) throw Error('Three reference points are required.');
  const [a,b,c]=points.map(p=>p.world);
  const det=(b.x-a.x)*(c.y-a.y)-(c.x-a.x)*(b.y-a.y);
  const spread=Math.max(Math.hypot(b.x-a.x,b.y-a.y),Math.hypot(c.x-a.x,c.y-a.y));
  if(!finite(det)||spread<100||Math.abs(det)<spread*spread*.02) throw Error('Choose points farther apart and not in a straight line.');
  const coeff=k=>{const [u,v,w]=points.map(p=>p.map[k]);const d=((v-u)*(c.y-a.y)-(w-u)*(b.y-a.y))/det;const e=((b.x-a.x)*(w-u)-(c.x-a.x)*(v-u))/det;return [d,e,u-d*a.x-e*a.y];};
  return [...coeff('x'),...coeff('y')];
}
export function heading(yaw,m){const r=yaw*Math.PI/180;return Math.atan2(m[3]*Math.cos(r)+m[4]*Math.sin(r),m[0]*Math.cos(r)+m[1]*Math.sin(r));}
export function validMatrix(m){return Array.isArray(m)&&m.length===6&&m.every(finite)&&Math.abs(m[0]*m[4]-m[1]*m[3])>1e-12;}
