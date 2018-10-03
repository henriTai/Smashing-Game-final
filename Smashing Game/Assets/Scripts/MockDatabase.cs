using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockDatabase : MonoBehaviour{

    public List<ScoreEntry> scores;
    public List<Player> players;
    public int maxSize;

    private void Awake()
    {
        scores = new List<ScoreEntry>();
        players = new List<Player>();
        maxSize = 5000;
        InitializeLists();
    }

    public EntryResult AddNewScore (NewEntry n)
    {
        bool isBanned = AddNewPlayer(n.name);
        if (isBanned)
        {
            EntryResult e = new EntryResult
            {
                banned = true,
                bestranking = 0,
                bestscore = 0,
                name = n.name,
                ranking = 0,
                score = 0
            };
            return e;
        }
        else
        {
            ScoreEntry entry = new ScoreEntry
            {
                _id = Guid.NewGuid(),
                name = n.name,
                score = n.score,
                date = DateTime.UtcNow
            };

            AddNewEntry(entry);

            List<Player> bannedPlayers = GetBannedPlayers(true);
            List<ScoreEntry> legalScores = new List<ScoreEntry>();

            EntryResult result = new EntryResult
            {
                name = n.name,
                banned = false,
                score = n.score
            };

            bool foundBest = false;
            bool foundThis = false;
            for (int i = 0; i < scores.Count; i++)
            {
                bool excluded = false;
                for (int j = 0; j < bannedPlayers.Count; j++)
                {
                    if (scores[i].name.Equals(bannedPlayers[j].name))
                    {
                        excluded = true;
                        break;
                    }
                }
                if (!excluded)
                {
                    legalScores.Add(scores[i]);
                    if (scores[i].name.Equals(entry.name))
                    {
                        if (!foundBest)
                        {
                            foundBest = true;
                            result.bestscore = scores[i].score;
                            result.bestranking = legalScores.Count;
                        }
                        if (scores[i]._id == entry._id)
                        {
                            foundThis = true;
                            result.score = scores[i].score;
                            result.ranking = legalScores.Count;
                        }
                    }
                }
                if (foundThis)
                {
                    break;
                }
            }
            return result;

        }
    }

    public EntryResult AddNewScoreInit(NewEntry n)
    {
        bool isBanned = AddNewPlayer(n.name);
        if (isBanned)
        {
            EntryResult e = new EntryResult
            {
                banned = true,
                bestranking = 0,
                bestscore = 0,
                name = n.name,
                ranking = 0,
                score = 0
            };
            return e;
        }
        else
        {
            int r = UnityEngine.Random.Range(0, 50);
            DateTime d = DateTime.UtcNow;
            d = d.AddDays(-r);

            ScoreEntry entry = new ScoreEntry
            {
                _id = Guid.NewGuid(),
                name = n.name,
                score = n.score,
                date = d

            };

            AddNewEntry(entry);

            List<Player> bannedPlayers = GetBannedPlayers(true);
            List<ScoreEntry> legalScores = new List<ScoreEntry>();

            EntryResult result = new EntryResult
            {
                name = n.name,
                banned = false,
                score = n.score
            };

            bool foundBest = false;
            bool foundThis = false;
            for (int i = 0; i < scores.Count; i++)
            {
                bool excluded = false;
                for (int j = 0; j < bannedPlayers.Count; j++)
                {
                    if (scores[i].name.Equals(bannedPlayers[j].name))
                    {
                        excluded = true;
                        break;
                    }
                }
                if (!excluded)
                {
                    legalScores.Add(scores[i]);
                    if (scores[i].name.Equals(entry.name))
                    {
                        if (!foundBest)
                        {
                            foundBest = true;
                            result.bestscore = scores[i].score;
                            result.bestranking = legalScores.Count;
                        }
                        if (scores[i]._id == entry._id)
                        {
                            foundThis = true;
                            result.score = scores[i].score;
                            result.ranking = legalScores.Count;
                        }
                    }
                }
                if (foundThis)
                {
                    break;
                }
            }
            return result;

        }
    }

    private List<ScoreEntry> GetLegalScores()
    {
        List<Player> bannedPlayers = GetBannedPlayers(true);
        List<ScoreEntry> legal = new List<ScoreEntry>();
        for (int i = 0; i < scores.Count; i++)
        {
            bool excluded = false;
            for (int j = 0; j < bannedPlayers.Count; j++)
            {
                if (bannedPlayers[j].name.Equals(scores[i].name))
                {
                    excluded = true;
                    break;
                }
            }
            if (!excluded)
            {
                legal.Add(scores[i]);
            }
        }
        return legal;
    }

    public ScoreEntry[]GetHighScores(int days)
    {
        DateTime searchDate = DateTime.UtcNow;
        searchDate = searchDate.AddDays(-days);
        if (days ==0)
        {
            return GetLegalScores().ToArray();
        }
        else
        {
            List<ScoreEntry> legal = GetLegalScores();
            List<ScoreEntry> entries = new List<ScoreEntry>();
            for (int i = 0; i < legal.Count; i++)
            {
                if (legal[i].date > searchDate)
                {
                    entries.Add(legal[i]);
                }
            }
            return entries.ToArray();
        }
    }

    public ScoreEntry[] GetPlayersScores(string name, int days)
    {
        DateTime searchDate = DateTime.UtcNow;
        searchDate = searchDate.AddDays(-days);
        List<ScoreEntry> entries = new List<ScoreEntry>();
        bool isBanned = IsPlayerBanned(name);
        if (isBanned)
        {
            return entries.ToArray();
        }
        else
        {
            List<ScoreEntry> legalScores = GetLegalScores();
            for (int i = 0; i < legalScores.Count; i++)
            {
                if (legalScores[i].name.Equals(name))
                {
                    if (days == 0)
                    {
                        entries.Add(legalScores[i]);
                    }
                    else
                    {
                        if (legalScores[i].date > searchDate)
                        {
                            entries.Add(legalScores[i]);
                        }
                    }
                }
            }
            return entries.ToArray();
        }
        
    }

    public bool IsPlayerBanned(string name)
    {
        bool banned = true;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].name.Equals(name))
            {
                banned = players[i].banned;
                break;
            }
        }
        return banned;
    }


    private void AddNewEntry(ScoreEntry s)
    {

        if (scores.Count < maxSize)
        {
            int index = scores.Count;
            for (int i = 0; i < scores.Count; i++)
            {
                if (s.score > scores[i].score)
                {
                    index = i;
                    break;
                }
            }
            scores.Insert(index, s);
        }
        else
        {
            if (s.score > scores[maxSize-1].score)
            {
                scores.RemoveAt(maxSize - 1);
                scores.Add(s);
            }
        }
    }

    private bool AddNewPlayer(string name)
    {
        bool found = false;
        bool banned = false;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].name.Equals(name))
            {
                found = true;
                banned = players[i].banned;
                break;
            }
        }
        if (!found)
        {
            Player p = new Player
            {
                name = name,
                banned = false,
                _id = Guid.NewGuid()
            };
            players.Add(p);
        }
        return banned;
    }

    public List<Player> GetBannedPlayers (bool isBanned)
    {
        List<Player> pl = new List<Player>();

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].banned == isBanned)
            {
                pl.Add(players[i]);
            }
 
        }

        return pl;
    }

    private void InitializeLists ()
    {
        String[] names = { "Aake", "Jamppa", "HenriLep", "Matti", "Teppo", "Seppo", "Timo", "Jari", "Ari", "Jake666"};
        NewEntry entry = new NewEntry();
        for (int i = 0; i < 200; i++)
        {
            entry.name = names[UnityEngine.Random.Range(0, names.Length-1)];
            entry.score = UnityEngine.Random.Range(40, 150);
            AddNewScoreInit(entry);
        }
    }

    public ScoreEntry[] GetScoresWithEntryResult(int slice )
    {
        List<Player> bannedPlayers = GetBannedPlayers(true);

        List<ScoreEntry> legalScores = new List<ScoreEntry>();

        for (int i = 0; i < scores.Count; i++)
        {
            bool excluded = false;
            for (int j = 0; j < bannedPlayers.Count; j++)
            {
                if (scores[i].name.Equals(bannedPlayers[j].name))
                {
                    excluded = true;
                    break;
                }
            }
            if (!excluded)
            {
                legalScores.Add(scores[i]);
            }
        }
        List<ScoreEntry> results = new List<ScoreEntry>();
        int resLength = 0;
        for (int i = slice; i < legalScores.Count; i++)
        {
            results.Add(legalScores[i]);
            resLength++;
            if (resLength == 10) break;
        }

        return results.ToArray();
    }

    public int GetCommonScore()
    {
        List<ScoreEntry> entries = GetLegalScores();
        Dictionary<int, int> dict = new Dictionary<int, int>();
        foreach (ScoreEntry s in entries)
        {
            int thisScore = s.score;
            int currentCount;
            dict.TryGetValue(thisScore, out currentCount);
            dict[thisScore] = currentCount + 1;
        }
        int max = 0;
        int key = 0;
        foreach (KeyValuePair<int, int> a in dict)
        {
            if (a.Value > max)
            {
                max = a.Value;
                key = a.Key;
            }
        }

        return key;
    }

    public float GetAverageScore()
    {
        List<ScoreEntry> entries = GetLegalScores();
        int sumOfScores = 0;
        foreach (ScoreEntry s in entries)
        {
            sumOfScores += s.score;
        }
        if (entries.Count == 0)
        {
            return 0f;
        }
        else return (float)sumOfScores / (float)entries.Count;

    }
}
