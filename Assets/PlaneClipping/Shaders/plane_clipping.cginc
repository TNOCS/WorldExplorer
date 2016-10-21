#ifndef PLANE_CLIPPING_INCLUDED
#define PLANE_CLIPPING_INCLUDED

//Plane clipping definitions. Uses three planes for clipping, but this can be increased if necessary.

#if CLIP_ONE || CLIP_TWO || CLIP_THREE
	//If we have 1, 2 or 3 clipping planes, PLANE_CLIPPING_ENABLED will be defined.
	//This makes it easier to check if this feature is available or not.
	#define PLANE_CLIPPING_ENABLED 1

	//http://mathworld.wolfram.com/Point-PlaneDistance.html
	float distanceToPlane(float3 planePosition, float3 planeNormal, float3 pointInWorld)
	{
	  //w = vector from plane to point
	  float3 w = - ( planePosition - pointInWorld );
	  float res = ( planeNormal.x * w.x + 
					planeNormal.y * w.y + 
					planeNormal.z * w.z ) 
		/ sqrt( planeNormal.x * planeNormal.x +
				planeNormal.y * planeNormal.y +
				planeNormal.z * planeNormal.z );
	  return res;
	}

	//we will have at least one plane.
	float4 _planePos;
	float4 _planeNorm;

	//at least two planes.
#if (CLIP_TWO || CLIP_THREE)
	float4 _planePos2;
	float4 _planeNorm2;
#endif

//at least three planes.
#if (CLIP_THREE)
	float4 _planePos3;
	float4 _planeNorm3;

	float4 _planePos4;
	float4 _planeNorm4;

	float4 _planePos5;
	float4 _planeNorm5;

	float4 _planePos6;
	float4 _planeNorm6;

	float4 _planePos7;
	float4 _planeNorm7;

	float4 _planePos8;
	float4 _planeNorm8;

	float4 _planePos9;
	float4 _planeNorm9;

	float4 _planePos10;
	float4 _planeNorm10;

	float4 _planePos11;
	float4 _planeNorm11;

	float4 _planePos12;
	float4 _planeNorm12;

	float4 _planePos13;
	float4 _planeNorm13;

	float4 _planePos14;
	float4 _planeNorm14;

	float4 _planePos15;
	float4 _planeNorm15;

	float4 _planePos16;
	float4 _planeNorm16;
#endif

	//discard drawing of a point in the world if it is behind any one of the planes.
	void PlaneClip(float3 posWorld) {
#if CLIP_THREE
	  clip(float3(
		distanceToPlane(_planePos.xyz, _planeNorm.xyz, posWorld),
		distanceToPlane(_planePos2.xyz, _planeNorm2.xyz, posWorld),
		distanceToPlane(_planePos3.xyz, _planeNorm3.xyz, posWorld)
	  ));
	  clip(float3(
		distanceToPlane(_planePos4.xyz, _planeNorm4.xyz, posWorld),
		distanceToPlane(_planePos5.xyz, _planeNorm5.xyz, posWorld),
		distanceToPlane(_planePos6.xyz, _planeNorm6.xyz, posWorld)
	  ));
	  clip(float3(
		distanceToPlane(_planePos7.xyz, _planeNorm7.xyz, posWorld),
		distanceToPlane(_planePos8.xyz, _planeNorm8.xyz, posWorld),
		distanceToPlane(_planePos9.xyz, _planeNorm9.xyz, posWorld)
	  ));
	  clip(float3(
		distanceToPlane(_planePos10.xyz, _planeNorm10.xyz, posWorld),
		distanceToPlane(_planePos11.xyz, _planeNorm11.xyz, posWorld),
		distanceToPlane(_planePos12.xyz, _planeNorm12.xyz, posWorld)
	  ));
	  clip(float3(
		distanceToPlane(_planePos13.xyz, _planeNorm13.xyz, posWorld),
		distanceToPlane(_planePos14.xyz, _planeNorm14.xyz, posWorld),
		distanceToPlane(_planePos15.xyz, _planeNorm15.xyz, posWorld)
	  ));
	  clip(distanceToPlane(_planePos16.xyz, _planeNorm16.xyz, posWorld));
#else //CLIP_THREE
#if CLIP_TWO
	  clip(float2(
		distanceToPlane(_planePos.xyz, _planeNorm.xyz, posWorld),
		distanceToPlane(_planePos2.xyz, _planeNorm2.xyz, posWorld)
	  ));
#else //CLIP_TWO
	  clip(distanceToPlane(_planePos.xyz, _planeNorm.xyz, posWorld));
#endif //CLIP_TWO
#endif //CLIP_THREE
	}

//preprocessor macro that will produce an empty block if no clipping planes are used.
#define PLANE_CLIP(posWorld) PlaneClip(posWorld);
    
#else
//empty definition
#define PLANE_CLIP(s)
#endif

#endif // PLANE_CLIPPING_INCLUDED