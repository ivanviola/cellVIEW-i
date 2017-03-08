using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public interface IItemInterface
{
	object[] Parameters {get; set;}

    void SetTextFontSize(int size);

    void SetContentAlpha(float alpha);

    bool GetLockState();

    bool GetSlowDownState();
}

