using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace AutoKeyboundBrawler.Core.Ipc;

internal sealed class NavmeshIpc
{
    private const string IsReadyFailed = "vnavmesh Nav.IsReady failed";

    private const string IsRunningFailed = "vnavmesh Path.IsRunning failed";

    private const string MoveFailed = "vnavmesh SimpleMove.PathfindAndMoveTo failed";

    private const string StopFailed = "vnavmesh Path.Stop failed";

    private readonly ICallGateSubscriber<bool> isReady;

    private readonly ICallGateSubscriber<bool> isRunning;

    private readonly ICallGateSubscriber<Vector3, bool, bool> pathfindAndMoveTo;

    private readonly ICallGateSubscriber<object> stop;

    private readonly Func<bool> isReadyCall;

    private readonly Func<bool> isRunningCall;

    private readonly Action stopCall;

    public bool Available => isReady.HasFunction;

    public NavmeshIpc(IDalamudPluginInterface pluginInterface)
    {
        isReady = pluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
        isRunning = pluginInterface.GetIpcSubscriber<bool>("vnavmesh.Path.IsRunning");
        pathfindAndMoveTo = pluginInterface.GetIpcSubscriber<Vector3, bool, bool>("vnavmesh.SimpleMove.PathfindAndMoveTo");
        stop = pluginInterface.GetIpcSubscriber<object>("vnavmesh.Path.Stop");
        isReadyCall = isReady.InvokeFunc;
        isRunningCall = isRunning.InvokeFunc;
        stopCall = stop.InvokeAction;
    }

    public bool IsReady() => IpcGate.Invoke(isReady.HasFunction, isReadyCall, fallback: false, IsReadyFailed);

    public bool IsRunning() => IpcGate.Invoke(isRunning.HasFunction, isRunningCall, fallback: false, IsRunningFailed);

    public bool PathfindAndMoveTo(Vector3 destination) => IpcGate.Invoke(pathfindAndMoveTo.HasFunction, () => pathfindAndMoveTo.InvokeFunc(destination, arg2: false), fallback: false, MoveFailed);

    public void Stop() => IpcGate.Run(stop.HasAction, stopCall, StopFailed);
}
