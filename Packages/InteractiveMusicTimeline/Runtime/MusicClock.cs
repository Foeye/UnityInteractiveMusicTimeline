/**
  * @file :MusicClock
  * @brief :时钟系统
  * @details : 用于DSP等需要精准时间的系统
  * @author :六队
  * @version :0.1
  * @date :2025年8月22日
  * @copyright :Foeye
 */

using UnityEngine;
using System;

public enum QuantizeUnit {
    None,
    Eighth,
    Quarter,
    Half,
    Bar
}

public sealed class MusicClock : MonoBehaviour {
    [Header("Tempo")]  
    public int bpm = 120;
    public int beatsPerBar = 4;

    public double BeatDuration => 60.0 / bpm;
    public double BarDuration => BeatDuration * beatsPerBar;

    private double dspStartTime;

    void Awake() {
        dspStartTime = AudioSettings.dspTime;
    }

    // 当前 DSP 时间到达的拍/小节相位（0..1）  
    public double GetBarPhase() {
        double elapsed = AudioSettings.dspTime - dspStartTime;
        double bars = elapsed / BarDuration;
        return bars - System.Math.Floor(bars);
    }

    public double NowDSP() => AudioSettings.dspTime;

    // 量化到下一拍/小节的 DSP 时间点  
    public double NextQuantizedDSP(QuantizeUnit q) {
        if (q == QuantizeUnit.None) return NowDSP() + 0.001;

        double unit = q switch {
            QuantizeUnit.Eighth => BeatDuration * 0.5,
            QuantizeUnit.Quarter => BeatDuration,
            QuantizeUnit.Half => BeatDuration * 2.0,
            QuantizeUnit.Bar => BarDuration,
            _ => BeatDuration
        };

        double now = NowDSP();
        double elapsedUnits = now / unit;
        double nextUnitIndex = System.Math.Ceiling(elapsedUnits + 1e-9);
        return nextUnitIndex * unit;
    }

    // 对齐若干单位后的 DSP 时间（例如“下两个小节后的落点”）  
    public double NextUnitsFromNow(QuantizeUnit unit, int count) {
        double step = unit switch {
            QuantizeUnit.Eighth => BeatDuration * 0.5,
            QuantizeUnit.Quarter => BeatDuration,
            QuantizeUnit.Half => BeatDuration * 2.0,
            QuantizeUnit.Bar => BarDuration,
            _ => BeatDuration
        };
        double first = NextQuantizedDSP(unit);
        return first + (count - 1) * step;
    }
}