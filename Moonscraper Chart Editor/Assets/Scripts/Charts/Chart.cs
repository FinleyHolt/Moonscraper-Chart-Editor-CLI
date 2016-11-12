﻿using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Chart  {
    Song song;
    List<ChartObject> _chartObjects;

    public ChartObject[] chartObjects { get { return _chartObjects.ToArray(); } }

    public ChartObject this[int i]
    {
        get { return _chartObjects[i]; }
        set { _chartObjects[i] = value; }
    }

    public int noteCount { get { return _chartObjects.OfType<Note>().Count(); } }
    public int Length { get { return _chartObjects.Count; } }
    public float endTime
    {
        get
        {
            float objectTime = _chartObjects[_chartObjects.Count - 1].time;

            return song.length > objectTime ? song.length : objectTime;
        }
    }

    public Chart (Song _song)
    {
        song = _song;
        _chartObjects = new List<ChartObject>();
    }

    // Insert into a sorted position
    // Return the position it was inserted into
    public int Add (ChartObject chartObject)
    {
        return SongObject.SortedInsert(chartObject, _chartObjects);
    }

    public bool Remove (ChartObject chartObject)
    {
        int pos = SongObject.FindObjectPosition(chartObject, _chartObjects.ToArray()); //BinarySearchChartExactNote(note);

        if (pos == Globals.NOTFOUND)
            return false;
        else
        {
            _chartObjects.RemoveAt(pos);
            return true;
        }
    }

    public ChartObject[] ToArray()
    {
        return _chartObjects.ToArray();
    }
    /*
    public Note FindPreviousNote (Note note)
    {
        // Binary search
        Note[] notes = chartObjects.ToArray().OfType<Note>().ToArray();
        int pos = SongObject.FindObjectPosition(note, notes);
        if (pos != Globals.NOTFOUND && pos > 0)
            return notes[pos - 1];
        else
            return null;
    }
    
    public Note FindNextNote (Note note)
    {
        // Binary search
        Note[] notes = chartObjects.ToArray().OfType<Note>().ToArray();
        int pos = SongObject.FindObjectPosition(note, notes.ToArray());
        if (pos != Globals.NOTFOUND && pos < notes.Length - 1)
            return notes[pos + 1];
        else
            return null;
    }
    */

    public Note[] GetNotes()
    {
        return _chartObjects.OfType<Note>().ToArray();
    }

    public void Load(List<string> data)
    {
        Load(data.ToArray());
    }

    public void Load(string[] data)
    {
        Regex noteRegex = new Regex(@"^\s*\d+ = N \d \d+$");            // 48 = N 2 0
        Regex starPowerRegex = new Regex(@"^\s*\d+ = S 2 \d+$");        // 768 = S 2 768
        Regex noteEventRegex = new Regex(@"^\s*\d+ = E \S");            // 1728 = E T

        List<string> flags = new List<string>();

        try
        {
            // Load notes, collect flags
            foreach (string line in data)
            {
                if (noteRegex.IsMatch(line))
                {
                    // Split string to get note information
                    string[] digits = Regex.Split(line.Trim(), @"\D+");

                    if (digits.Length == 3)
                    {
                        uint position = uint.Parse(digits[0]);
                        int fret_type = int.Parse(digits[1]);
                        uint length = uint.Parse(digits[2]);

                        // Collect flags
                        if (fret_type > 4 || fret_type < 0)
                        {
                            flags.Add(line);
                        }
                        else
                        {
                            // Add note to the data
                            Note newNote = new Note(song, this, position, (Note.Fret_Type)fret_type, length);
                            Add(newNote);
                        }
                    }
                }
                
                else if (starPowerRegex.IsMatch(line))
                {
                    string[] digits = Regex.Split(line.Trim(), @"\D+");

                    uint position = uint.Parse(digits[0]);
                    uint length = uint.Parse(digits[2]);

                    Add(new StarPower(song, this, position, length));
                }
                
                else if (noteEventRegex.IsMatch(line))
                {
                    string[] strings = Regex.Split(line.Trim(), @"\s+");

                    uint position = uint.Parse(strings[0]);
                    string eventName = strings[3];

                    Add(new ChartEvent(song, this, position, eventName));
                }
            }

            // Load flags
            foreach (string line in flags)
            {
                if (noteRegex.IsMatch(line))
                {
                    // Split string to get note information
                    string[] digits = Regex.Split(line.Trim(), @"\D+");

                    if (digits.Length == 3)
                    {
                        int position = int.Parse(digits[0]);
                        int fret_type = int.Parse(digits[1]);

                        Note[] notesToFlag = SongObject.FindObjectsAtPosition(position, _chartObjects.OfType<Note>().ToArray());

                        // TODO
                        if (fret_type > 4 || fret_type < 0)
                        {
                            switch (fret_type)
                            {
                                case (5):
                                    Note.groupAddFlags(notesToFlag, Note.Flags.FORCED);
                                    break;
                                case (6):
                                    Note.groupAddFlags(notesToFlag, Note.Flags.TAP);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            // Bad load, most likely a parsing error
            Debug.LogError(e.Message);
            _chartObjects.Clear();
        }
    }

    public string GetChartString()
    {
        string chart = string.Empty;

        for(int i = 0; i < _chartObjects.Count; ++i)
        {
            chart += _chartObjects[i].GetSaveString();

            if (_chartObjects[i].GetType() == typeof(Note))
            {
                // if the next note is not at the same position, add flags
                Note currentNote = (Note)_chartObjects[i];    
                //Note nextNote = FindNextNote(i);
                Note nextNote = (Note)SongObject.FindNext(typeof(Note), i, chartObjects);

                if (nextNote != null && nextNote.position != currentNote.position)
                    chart += currentNote.GetFlagsSaveString();
            }
        }

        return chart;
    }
}
