using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

class MusicPlayerComponent : MonoBehaviour {

    private const string VALID_NOTES = "CcDdEFfGgAaB_Oo";

    private readonly Dictionary<char, float> NOTE_PITCH_MULTIPLIERS = new Dictionary<char, float>(){
        {'C', 1.000f},
        {'c', 1.059f},
        {'D', 1.122f},
        {'d', 1.189f},
        {'E', 1.259f},
        {'F', 1.337f},
        {'f', 1.414f},
        {'G', 1.498f},
        {'g', 1.587f},
        {'A', 1.681f},
        {'a', 1.781f},
        {'B', 1.887f},
    };

    public TextAsset testMusicSheet;

    public AudioClip[] instrumentSamples;

    private bool playing;
    private int noteIndex = 0;

    private int maxTrack;
    private Dictionary<int, List<char>> tracks;
    private Dictionary<int, AudioSource> instrumentSources;
    private Dictionary<int, float> currentOctaves = new Dictionary<int, float>();

    private Timer tempoTimer = new Timer();

    void Start(){
        ParseTracks(testMusicSheet);
        SetupInstruments();

        playing = true;
    }

    void Update(){
        if(playing){
            // Articificial rolloff clamping sounds... lessens pop but ruins sample...
            // float t = 1.0f - tempoTimer.Parameterized();
            // foreach(KeyValuePair<int, AudioSource> kvp in instrumentSources){
            //     kvp.Value.volume = t;
            // }

            if(tempoTimer.Finished()){
                foreach(KeyValuePair<int, List<char>> kvp in tracks){
                    if(noteIndex < kvp.Value.Count){
                        char note = kvp.Value[noteIndex];

                        if(note == 'o' || note == 'O'){
                            currentOctaves[kvp.Key] *= (note == 'O' ? 2.0f : 0.5f);
                        } else if(note != '_'){
                            instrumentSources[kvp.Key].pitch = NOTE_PITCH_MULTIPLIERS[note] * currentOctaves[kvp.Key];
                            instrumentSources[kvp.Key].Play();
                        }
                    }
                }

                noteIndex++;
                if(noteIndex >= maxTrack){
                    noteIndex = 0;

                    currentOctaves.Clear();
                    for(int i = 0, count = instrumentSamples.Length; i < count; ++i){
                        currentOctaves[i] = 1.0f;
                    }
                }
                tempoTimer.Start();
            }
        }
    }

    private void ParseTracks(TextAsset musicSheet){
        playing = false;
        noteIndex = 0;
        maxTrack = 0;

        tracks = new Dictionary<int, List<char>>();

        string[] trackTokens = musicSheet.text.Trim().Split('\n');

        tempoTimer.SetDuration(float.Parse(trackTokens[0].Trim()) / 1000.0f);

        for(int i = 1, count = trackTokens.Length; i < count; ++i){
            string[] instrumentAndNotes = trackTokens[i].Split(':');

            int instrumentValue = int.Parse(instrumentAndNotes[0]);

            if(!tracks.ContainsKey(instrumentValue)){
                tracks[instrumentValue] = new List<char>();
            }

            if(!currentOctaves.ContainsKey(instrumentValue)){
                currentOctaves[instrumentValue] = 1.0f;
            }

            foreach(char note in instrumentAndNotes[1]){
                if(VALID_NOTES.IndexOf(note) >= 0){ // -1 if not found; contains() wasn't working
                    tracks[instrumentValue].Add(note);
                }

                if(tracks[instrumentValue].Count > maxTrack){
                    maxTrack = tracks[instrumentValue].Count;
                }
            }
        }
    }

    private void SetupInstruments(){
        instrumentSources = new Dictionary<int, AudioSource>();

        for(int i = 0, count = instrumentSamples.Length; i < count; ++i){
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = instrumentSamples[i];

            instrumentSources[i] = newSource;
        }
    }
}