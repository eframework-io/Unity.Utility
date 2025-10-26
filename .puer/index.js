// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

//#region XObject
const XObjectThis = "__xobject_this"

CS.EFramework.Unity.Utility.XObject.This = function () {
    return function (target, propertyKey) {
        target[XObjectThis] = target[XObjectThis] || new Array()
        target[XObjectThis].push(propertyKey)
    }
}

function HashCode(obj) {
    let hash = 0

    if (obj == null) return hash
    if (obj.GetHashCode) return obj.GetHashCode()
    if (typeof obj === "boolean") return obj ? 1 : 0
    if (typeof obj === "number") hash = Math.floor(obj)
    if (typeof obj === "string") {
        for (let i = 0; i < obj.length; i++) {
            const chr = obj.charCodeAt(i)
            hash = ((hash << 5) - hash) + chr
            hash |= 0
        }
    }

    if (Array.isArray(obj)) {
        for (let i = 0; i < obj.length; i++) {
            hash = ((hash << 5) - hash) + HashCode(obj[i])
            hash |= 0
        }
    }

    if (typeof obj === "object") {
        const keys = Object.keys(obj).sort()
        for (let i = 0; i < keys.length; i++) {
            const key = keys[i]
            const keyHash = HashCode(key)
            const valueHash = HashCode(obj[key])
            hash = ((hash << 5) - hash) + keyHash
            hash |= 0
            hash = ((hash << 5) - hash) + valueHash
            hash |= 0
        }
    }

    return Math.abs(hash)
}

CS.EFramework.Unity.Utility.XObject.HashCode = HashCode
CS.EFramework.Unity.Utility.XObject.TypeOf = puer.$typeof
//#endregion

//#region XString
CS.EFramework.Unity.Utility.XString.Format = function (fmt, ...args) {
    if (fmt) {
        if (args.length > 0) {
            let index = 0
            const doReplace = (rplc) => {
                if (rplc == null) rplc = "undefined"
                if (Array.isArray(rplc)) {
                    for (let i = 0; i < rplc.length; i++) {
                        let temp = rplc[i]
                        doReplace(temp)
                    }
                } else {
                    let str
                    let reg = new RegExp("\\{" + index + "\\}", "g")
                    if (typeof rplc === "string") {
                        str = rplc
                    } else {
                        str = rplc.toString()
                    }
                    fmt = fmt.replace(reg, str)
                    index++
                }
            }
            for (let i = 0; i < args.length; i++) {
                let temp = args[i]
                if (temp != null) {
                    doReplace(temp)
                }
            }
        }
        return fmt
    } else {
        return null
    }
}
//#endregion

//#region XLog
const _Emergency = CS.EFramework.Unity.Utility.XLog.Emergency
const _Alert = CS.EFramework.Unity.Utility.XLog.Alert
const _Critical = CS.EFramework.Unity.Utility.XLog.Critical
const _Error = CS.EFramework.Unity.Utility.XLog.Error
const _Warn = CS.EFramework.Unity.Utility.XLog.Warn
const _Notice = CS.EFramework.Unity.Utility.XLog.Notice
const _Info = CS.EFramework.Unity.Utility.XLog.Info
const _Debug = CS.EFramework.Unity.Utility.XLog.Debug

const isDebug = CS.EFramework.Unity.Utility.XEnv.Mode == CS.EFramework.Unity.Utility.XEnv.ModeType.Dev ||
    CS.UnityEngine.Debug.isDebugBuild ||
    CS.UnityEngine.Application.isEditor

function genLink(trace) {
    let regex = /at ([a-zA-z0-9#$._ ]+ \()?([^\n\r\*\"\|\<\>]+(.js|.cjs|.mjs|.ts|.mts))\:([0-9]+)\:([0-9]+)\)?/g
    for (let i = 0; i < trace.length; i++) {
        regex.lastIndex = 0
        let match = regex.exec(trace[i])
        if (!match) continue
        let path = match[2], line = match[4] ?? "0", column = match[5] ?? "0"
        let search = `${path}:${line}:${column}`
        let npath = path.replace(/\\\\/g, "/").replace(/\\/g, "/")
        let nsearch = `${npath}:${line}:${column}`
        trace[i] = trace[i].replace(search, `<a href="${npath}" line="${line}" column="${column}">${nsearch}</a>`)
    }
}

function genTrace() {
    let trace = new Error().stack?.replace(/\r\n/g, "\n").split("\n").slice(3)
    if (trace && trace.length > 0) {
        genLink(trace)
        return trace.join("\n")
    }
    return ""
}

CS.EFramework.Unity.Utility.XLog.Emergency = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Emergency(...args)
}

CS.EFramework.Unity.Utility.XLog.Alert = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Alert(...args)
}

CS.EFramework.Unity.Utility.XLog.Critical = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Critical(...args)
}

CS.EFramework.Unity.Utility.XLog.Error = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Error(...args)
}

CS.EFramework.Unity.Utility.XLog.Warn = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Warn(...args)
}

CS.EFramework.Unity.Utility.XLog.Notice = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Notice(...args)
}

CS.EFramework.Unity.Utility.XLog.Info = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Info(...args)
}

CS.EFramework.Unity.Utility.XLog.Debug = function (...args) {
    if (isDebug) {
        if (args != null && args.length > 0 && typeof (args[0]) == "string") {
            args[0] += "\n" + genTrace()
        }
    }
    _Debug(...args)
}
//#endregion

//#region XComp
const _GetTransform = CS.EFramework.Unity.Utility.XComp.GetTransform
const _GetComponent = CS.EFramework.Unity.Utility.XComp.GetComponent
const _GetComponentInParent = CS.EFramework.Unity.Utility.XComp.GetComponentInParent
const _GetComponentInChildren = CS.EFramework.Unity.Utility.XComp.GetComponentInChildren
const _GetComponents = CS.EFramework.Unity.Utility.XComp.GetComponents
const _GetComponentsInParent = CS.EFramework.Unity.Utility.XComp.GetComponentsInParent
const _GetComponentsInChildren = CS.EFramework.Unity.Utility.XComp.GetComponentsInChildren
const _RemoveComponent = CS.EFramework.Unity.Utility.XComp.RemoveComponent
const _AddComponent = CS.EFramework.Unity.Utility.XComp.AddComponent
const _SetComponentEnabled = CS.EFramework.Unity.Utility.XComp.SetComponentEnabled
const MonoBehaviour = CS.UnityEngine.MonoBehaviour

CS.EFramework.Unity.Utility.XComp.GetComponent = function (parentObj, pathOrType, typeOrAttach = false, attachIfMissing = false) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof typeOrAttach === "function") return getComponent(root.gameObject, typeOrAttach, attachIfMissing)
        else return _GetComponent(root, typeOrAttach, attachIfMissing)
    } else {
        if (typeof pathOrType === "function") return getComponent(parentObj, pathOrType, typeOrAttach)
        else return _GetComponent(parentObj, pathOrType, typeOrAttach)
    }
}

CS.EFramework.Unity.Utility.XComp.GetComponentInParent = function (parentObj, pathOrType, typeOrInclude = false, includeInactive = false) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof typeOrInclude === "function") return getComponentInParent(root.gameObject, typeOrInclude, includeInactive)
        else return _GetComponentInParent(root, typeOrInclude, includeInactive)
    } else {
        if (typeof pathOrType === "function") return getComponentInParent(parentObj, pathOrType, typeOrInclude)
        else return _GetComponentInParent(parentObj, pathOrType, typeOrInclude)
    }
}

CS.EFramework.Unity.Utility.XComp.GetComponentInChildren = function (parentObj, pathOrType, typeOrInclude = false, includeInactive = false) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof typeOrInclude === "function") return getComponentInChildren(root.gameObject, typeOrInclude, includeInactive)
        else return _GetComponentInChildren(root, typeOrInclude, includeInactive)
    } else {
        if (typeof pathOrType === "function") return getComponentInChildren(parentObj, pathOrType, typeOrInclude)
        else return _GetComponentInChildren(parentObj, pathOrType, typeOrInclude)
    }
}

CS.EFramework.Unity.Utility.XComp.GetComponents = function (parentObj, pathOrType, type) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof type === "function") return getComponents(root.gameObject, type)
        else return _GetComponents(root, type)
    } else {
        if (typeof pathOrType === "function") return getComponents(parentObj, pathOrType)
        else return _GetComponents(parentObj, pathOrType)
    }
}

CS.EFramework.Unity.Utility.XComp.GetComponentsInParent = function (parentObj, pathOrType, typeOrInclude = false, includeInactive = false) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof typeOrInclude === "function") return getComponentsInParent(root.gameObject, typeOrInclude)
        else return _GetComponentsInParent(root, typeOrInclude, includeInactive)
    } else {
        if (typeof pathOrType === "function") return getComponentsInParent(parentObj, pathOrType)
        else return _GetComponentsInParent(parentObj, pathOrType, typeOrInclude)
    }
}

CS.EFramework.Unity.Utility.XComp.GetComponentsInChildren = function (parentObj, pathOrType, typeOrInclude = false, includeInactive = false) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof typeOrInclude === "function") return getComponentsInChildren(root.gameObject, typeOrInclude)
        else return _GetComponentsInChildren(root, typeOrInclude, includeInactive)
    } else {
        if (typeof pathOrType === "function") return getComponentsInChildren(parentObj, pathOrType)
        else return _GetComponentsInChildren(parentObj, pathOrType, typeOrInclude)
    }
}

CS.EFramework.Unity.Utility.XComp.RemoveComponent = function (parentObj, pathOrType, typeOrImmediate = false, immediate = false) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof typeOrImmediate === "function") return removeComponent(root.gameObject, typeOrImmediate, immediate)
        else return _RemoveComponent(root, typeOrImmediate, immediate)
    } else {
        if (typeof pathOrType === "function") return removeComponent(parentObj, pathOrType, typeOrImmediate)
        else return _RemoveComponent(parentObj, pathOrType, typeOrImmediate)
    }
}

CS.EFramework.Unity.Utility.XComp.AddComponent = function (parentObj, pathOrType, type) {
    if (typeof pathOrType === "string") {
        if (typeof type === "function") return MonoBehaviour.Add(parentObj, pathOrType, type)
        else return _AddComponent(root, type)
    } else {
        if (typeof pathOrType === "function") return MonoBehaviour.Add(parentObj, null, pathOrType)
        else return _AddComponent(parentObj, pathOrType)
    }
}

CS.EFramework.Unity.Utility.XComp.SetComponentEnabled = function (parentObj, pathOrType, typeOrEnabled, enabled) {
    if (typeof pathOrType === "string") {
        let root = _GetTransform(parentObj, pathOrType)
        if (typeof typeOrEnabled === "function") return setComponentEnabled(root.gameObject, typeOrEnabled, enabled)
        else return _SetComponentEnabled(root, typeOrEnabled, enabled)
    } else {
        if (typeof pathOrType === "function") return setComponentEnabled(parentObj, pathOrType, typeOrEnabled)
        else return _SetComponentEnabled(parentObj, pathOrType, typeOrEnabled)
    }
}

function getComponent(obj, type, attachIfMissing = false) {
    let comp = MonoBehaviour.Get(obj, type)
    if (comp != null) return comp.JProxy
    else if (attachIfMissing) {
        return _AddComponent(obj, type)
    }
}

function getComponentInParent(obj, type, includeInactive = false) {
    let comp = MonoBehaviour.GetInParent(obj, type, includeInactive)
    return comp?.JProxy
}

function getComponentInChildren(obj, type, includeInactive = false) {
    let comp = MonoBehaviour.GetInChildren(obj, type, includeInactive)
    return comp?.JProxy
}

function getComponents(obj, type) {
    let ncomps = []
    let comps = MonoBehaviour.Gets(obj, type)
    if (comps != null && comps.Length > 0) {
        for (let i = 0; i < comps.Length; ++i) {
            ncomps.push(comps.get_Item(i)?.JProxy)
        }
    }
    return ncomps
}

function getComponentsInParent(obj, type) {
    let ncomps = []
    let comps = MonoBehaviour.GetsInParent(obj, type)
    if (comps != null && comps.Length > 0) {
        for (let i = 0; i < comps.Length; ++i) {
            ncomps.push(comps.get_Item(i)?.JProxy)
        }
    }
    return ncomps
}

function getComponentsInChildren(obj, type) {
    let ncomps = []
    let comps = MonoBehaviour.GetsInChildren(obj, type)
    if (comps != null && comps.Length > 0) {
        for (let i = 0; i < comps.Length; ++i) {
            ncomps.push(comps.get_Item(i)?.JProxy)
        }
    }
    return ncomps
}

function removeComponent(obj, type, immediate) {
    let comp = MonoBehaviour.Get(obj, type)
    if (comp != null) {
        if (immediate) CS.UnityEngine.Object.DestroyImmediate(comp)
        else CS.UnityEngine.Object.Destroy(comp)
    }
}

function setComponentEnabled(obj, type, enabled) {
    let comp = MonoBehaviour.Get(obj, type)
    if (comp != null) {
        comp.enabled = enabled
        return comp.JProxy
    }
    else return null
}
//#endregion

//#region XEvent
const XLog = CS.EFramework.Unity.Utility.XLog
const XEvent = CS.EFramework.Unity.Utility.XEvent
const Manager = XEvent.Manager

XEvent.Manager = class {
    constructor(multiple = true) {
        this.CProxy = new Manager(multiple)
        this.multiple = multiple
        this.callbacks = new Map()
        this.onces = new Map()
        this.map = new Map()
    }

    Register(id, callback, once = false) {
        if (!callback) {
            XLog.Error(`XEvent.Manager.Register: nil callback, id=${id}`)
            return false
        }

        let callbacks = this.callbacks.get(id)
        if (callbacks == null) {
            callbacks = new Array()
            this.callbacks.set(id, callbacks)
        }

        if (!this.multiple && callbacks.length > 1) {
            XLog.Error(`XEvent.Manager.Register: not support multi-register, id=${id}`)
            return false
        }

        for (let i = 0; i < callbacks.length; i++) {
            if (callbacks[i] == callback) return false
        }

        if (once) this.onces.set(callback, once)
        callbacks.push(callback)

        const that = this
        function ncallback(args) {
            if (that && that.onces) {
                let once = that.onces.get(callback)
                if (once) {
                    delete that.onces.delete(callback)
                    that.Unregister(id, callback)
                }
            }

            if (args && args.Length > 0) {
                let nargs = []
                for (let i = 0; i < args.Length; i++) {
                    nargs.push(args.get_Item(i))
                }
                callback(...nargs)
            } else callback()
        }

        this.map.set(callback, ncallback)
        if (this.CProxy) this.CProxy.Register(id, ncallback, once ? true : false)

        return true
    }

    Unregister(id, callback = null) {
        let ret = false
        const callbacks = this.callbacks.get(id)
        if (callbacks && callbacks.length > 0) {
            for (let i = 0; i < callbacks.length;) {
                let ele = callbacks[i]
                if (callback == null || ele == callback) {
                    callbacks.splice(i, 1)
                    let ocallback = this.map.get(ele)
                    if (ocallback) {
                        this.map.delete(ele)
                        if (this.onces.has(ele)) this.onces.delete(ele)
                        if (this.CProxy) this.CProxy.Unregister(id, ocallback)
                    }
                } else i++
            }
            if (callbacks.length == 0) this.callbacks.delete(id)
        }
        return ret
    }

    Notify(id, ...args) { this.CProxy.Notify(id, ...args) }

    Clear() {
        this.callbacks = new Map()
        this.onces = new Map()
        this.map = new Map()
        this.CProxy.Clear()
    }
}
//#endregion
