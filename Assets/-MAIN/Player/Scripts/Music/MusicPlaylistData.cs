using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New MusicPlaylist Data", menuName = "MusicPlaylists/Playlist Data")]
public class MusicPlaylistData : ScriptableObject
{
    [System.Serializable]
    public class Song
    {
        [HideInInspector] public string filePath; 
        public AudioClip audioClip;
    }

    public Texture2D playListImage;
    public string playListName; 
    public List<Song> songs = new List<Song>();
}