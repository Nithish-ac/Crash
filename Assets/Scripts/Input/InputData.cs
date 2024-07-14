using Fusion;
using UnityEngine;

[System.Flags]
public enum InputButton
{
    LEFT,
    RIGHT,
    UP, 
    DOWN
}

public struct InputData : INetworkInput
{
    public NetworkButtons Buttons;

    public bool GetButton(InputButton button)
    {
        return Buttons.IsSet(button);
    }

    public NetworkButtons GetButtonPressed(NetworkButtons prev)
    {
        return Buttons.GetPressed(prev);
    }

    public bool AxisPressed()
    {
        return GetButton(InputButton.LEFT) || GetButton(InputButton.RIGHT);
    }
}
