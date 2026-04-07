/**
 * WebGL stub for Fork Particle Variety Pack FX native plugin.
 * All functions are no-ops / return safe defaults so the WASM linker
 * can resolve the __Internal DllImport symbols.  Fork Particle effects
 * will simply be invisible in the browser build.
 *
 * _frkParticlePluginSDKInit returns 0 (false) which prevents
 * ForkParticleEffect from ever calling CreateEffect, so no further
 * native calls are made at runtime.
 */
var LibraryForkParticle = {
    _frkParticlePluginSDKInit: function () { return 0; },
    _frkParticlePluginSDKShutdown: function () { return 1; },
    _frkParticlePluginSetEffectView: function () {},
    _frkParticlePluginSDKProcess: function () { return 1; },

    _frkParticlePluginCreateEffect: function () { return 0; },
    _frkParticlePluginGetEffectTexturePointer: function () {},
    _frkParticlePluginSetEffectMatrix: function () {},
    _frkParticlePluginSetEffectColor: function () {},
    _frkParticlePluginSetEffectScale: function () {},
    _frkParticlePluginDestroyEffect: function () {},
    _frkParticlePluginClearEffect: function () {},
    _frkParticlePluginEffectIsAlive: function () { return 0; },
    _frkParticlePluginEffectFreeze: function () {},
    _frkParticlePluginPlayEffect: function () {},
    _frkParticlePluginSetEffectEnable: function () {},
    _frkParticlePluginGetEffectRenderParamsNumIndices: function () { return 0; },
    _frkParticlePluginGetEffectRenderParamsNumVertices: function () { return 0; },
    _frkParticlePluginGetEffectRenderParams: function () {},
    _frkParticlePluginGetEffectNumEmitters: function () { return 0; },
    _frkParticlePluginEffectGetGeometry: function () { return 0; },
    _frkParticlePluginEffectRestart: function () {},
    _frkParticlePluginEffectPrepareGeometry: function () { return 0; },

    RegisterSetTextureNameCB: function () {}
};
mergeInto(LibraryManager.library, LibraryForkParticle);
