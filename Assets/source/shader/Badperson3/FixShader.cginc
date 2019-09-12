#ifndef AUTOLIGHT_FIXES_INCLUDED
#define AUTOLIGHT_FIXES_INCLUDED

#include "HLSLSupport.cginc"
#include "UnityShadowLibrary.cginc"

// Problem 1: SHADOW_COORDS - undefined identifier.
// Why: Using SHADOWS_DEPTH without SPOT.
// The file AutoLight.cginc only takes into account the case where you use SHADOWS_DEPTH + SPOT (to enable SPOT just add a Spot Light in the scene).
// So, if your scene doesn't have a Spot Light, it will skip the SHADOW_COORDS definition and shows the error.
// Now, to workaround this you can:
// 1. Add a Spot Light to your scene
// 2. Use this CGINC to workaround this scase.  Also, you can copy this in your own shader.
#if defined (SHADOWS_DEPTH) && !defined (SPOT)
#       define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
#endif


// Problem 2: _ShadowCoord - invalid subscript.
// Why: nor Shadow screen neighter Shadow Depth or Shadow Cube and trying to use _ShadowCoord attribute.
// The file AutoLight.cginc defines SHADOW_COORDS to empty when no one of these options are enabled (SHADOWS_SCREEN, SHADOWS_DEPTH and SHADOWS_CUBE),
// So, if you try to call "o._ShadowCoord = ..." it will break because _ShadowCoord isn't an attribute in your structure.
// To workaround this you can:
// 1. Check if one of those defines actually exists in any place where you have "o._ShadowCoord...".
// 2. Use the define SHADOWS_ENABLED from this file to perform the same check.
#if defined (SHADOWS_SCREEN) || defined (SHADOWS_DEPTH) || defined (SHADOWS_CUBE)
#    define SHADOWS_ENABLED
#endif

#endif