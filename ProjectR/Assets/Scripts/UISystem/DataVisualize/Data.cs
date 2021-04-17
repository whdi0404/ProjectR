using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Data
{
    private List<DataSetter> dataSetters = new List<DataSetter>();

    public abstract int ToByte();

    public abstract int ToShort();

    public abstract int ToInt();

    public abstract long ToLong();

    public abstract float ToFloat();

    public abstract double ToDouble();

    public void RegistSetter(DataSetter dataSetter)
    {
        dataSetters.Add(dataSetter);
    }

    public void UnregistSetter(DataSetter dataSetter)
    {
        dataSetters.Remove(dataSetter);
    }

    public void OnChangeValue()
    {
        foreach (var data in dataSetters)
        {
            data.SetValue(this);
        }
    }
}

public class Data<TValue> : Data
{
    private TValue value;

    public TValue Value {
        get => value;
        set 
        {
            this.value = value;
            OnChangeValue();
        }
    }

    public override int ToByte()
    {
        return Convert.ToByte(value);
    }

    public override int ToShort()
    {
        return Convert.ToInt16(value);
    }

    public override int ToInt()
    {
        return Convert.ToInt32(value);
    }

    public override long ToLong()
    {
        return Convert.ToInt64(value);
    }

    public override float ToFloat()
    {
        return Convert.ToSingle(value);
    }

    public override double ToDouble()
    {
        return Convert.ToDouble(value);
    }

    public override string ToString()
    {
        return value.ToString();
    }
}
