using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Agg.AdaptiveSubdivision.VisualTest;

[SuppressMessage("ReSharper", "LocalizableElement")]
public class SingletonForm : Form
{

    protected static T? ShowInstance<T>(Form mdiParent)
        where T : SingletonForm
    {
        var type = typeof(T);

        SingletonForm instance;

        if (Instances.ContainsKey(type))
        {
            instance = Instances[type];
            instance.Activate();
            return (T)instance;
        }

        if (mdiParent is not { IsMdiContainer: true })
        {
            throw new ArgumentException("Invalid MDI parent.", nameof(mdiParent));
        }

        if (!type.IsSubclassOf(typeof(Form)))
        {
            throw new InvalidCastException();
        }

        var ctor = type.GetConstructor(Type.EmptyTypes);

        if (ctor == null)
        {
            Debug.Print("Warning: not able to access public parameterless constructor.");
            return null;
        }

        var obj = ctor.Invoke(Array.Empty<object>());

        instance = (SingletonForm)obj;
        instance.MdiParent = mdiParent;

        Instances[type] = instance;

        instance.Show();

        return (T)instance;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Instances.Remove(GetType());

        base.OnFormClosed(e);
    }

    private static readonly Dictionary<Type, SingletonForm> Instances = new();

}
