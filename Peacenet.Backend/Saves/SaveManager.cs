﻿using System;
using Plex.Objects;
using System.Collections.Generic;
using System.IO;
using Whoa;
using System.Text;
using System.Linq;
using LiteDB;
using Newtonsoft.Json;

namespace Peacenet.Backend.Saves
{
    /// <summary>
    /// Provides extremely simple save management for the Peacenet server.
    /// </summary>
    public class SaveManager : IBackendComponent
    {
        private LiteCollection<SaveFile> _saves = null;

        private LiteCollection<SaveValue> _values = null;

        [Dependency]
        private DatabaseHolder _db = null;

        /// <inheritdoc/>
        public void Initiate()
        {
            Logger.Log("Save manager is starting...");
            _saves = _db.Database.GetCollection<SaveFile>("usersaves");
            _saves.EnsureIndex(x => x.Id);
            _values = _db.Database.GetCollection<SaveValue>("usersavevalues");
            _values.EnsureIndex(x => x.Id);
            Logger.Log($"Done. {_saves.Count()} saves loaded. {_values.Count()} total values loaded.");
        }

        /// <summary>
        /// Determines if a user has a save file on this server
        /// </summary>
        /// <param name="session">The user ID of the user to look up</param>
        /// <returns>Whether or not the user has a save</returns>
        public bool UserHasSave(string session)
        {
            var save = _saves.FindOne(x => x.UserId == session);
            return (save != null);
        }

        /// <summary>
        /// Retrieve information about the save file for the specified Watercolor user.
        /// </summary>
        /// <param name="session">The ID of the user to look up</param>
        /// <returns>The save file information found in the server database. If the user doesn't already have a save file, a new one will be created and the method will return that save file.</returns>
        public SaveFile GetSave(string session)
        {
            if (UserHasSave(session))
            {
                return _saves.FindOne(x => x.UserId == session);
            }
            var save = new SaveFile
            {
                Id = Guid.NewGuid().ToString(),
                UserId = session,
            };
            _saves.Insert(save);
            return save;
        }

        /// <summary>
        /// Retrieve the value of a save entry.
        /// </summary>
        /// <param name="session">The user ID for the save you'd like to look up[</param>
        /// <param name="key">The friendly key for the entry you'd like to look up. If no entry is found, one will be created.</param>
        /// <param name="defaultValue">The default value to set the new entry to if the entry you're requesting doesn't exist.</param>
        /// <returns>The value of the requested entry.</returns>
        public string GetValue(string session, string key, string defaultValue)
        {
            var save = GetSave(session);

            var val = _values.FindOne(x => x.SaveId == save.Id && x.Key == key);
            if (val != null)
                return val.Value;
            val = new SaveValue
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                SaveId = save.Id,
                Value = defaultValue,
            };
            _values.Insert(val);
            return val.Value;

        }

        /// <summary>
        /// Set the value of an entry in a save file.
        /// </summary>
        /// <param name="session">The user ID for the save you want to modify</param>
        /// <param name="key">The friendly key of the entry. If an entry with this key doesn't exist, one will be created.</param>
        /// <param name="value">The value you'd like to set in the entry.</param>
        public void SetValue(string session, string key, string value)
        {
            var save = GetSave(session);

            var val = _values.FindOne(x => x.SaveId == save.Id && x.Key == key);
            if (val != null)
            {
                val.Value = value;
                _values.Update(val);
                return;
            }
            val = new SaveValue
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                SaveId = save.Id,
                Value = value,
            };
            _values.Insert(val);
        }

        /// <inheritdoc/>
        public void SafetyCheck()
        {
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }
    }

    /// <summary>
    /// Represents a server-side save file in a database.
    /// </summary>
    public class SaveFile
    {
        /// <summary>
        /// Gets or sets the unique row ID for the save file.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets this save's associated Watercolor user ID.
        /// </summary>
        public string UserId { get; set; }
    }

    /// <summary>
    /// Represents an entry in a server-side <see cref="SaveFile"/>. 
    /// </summary>
    public class SaveValue
    {
        /// <summary>
        /// Gets or sets the unique row ID of the save entry.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the ID of the save file to which this entry belongs.
        /// </summary>
        public string SaveId { get; set; }
        /// <summary>
        /// Gets or sets the public, friendly entry ID used by the Peace engine to find the save value. This must also be unique.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the actual value contained in the save entry.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Handler for retrieving save values
    /// </summary>
    [RequiresSession]
    public class SaveGetValueHandler : IMessageHandler
    {
        /// <inheritdoc/>
        public ServerMessageType HandledMessageType
        {
            get
            {
                return ServerMessageType.SAVE_GETVAL;
            }
        }

        /// <inheritdoc/>
        public ServerResponseType HandleMessage(Backend backend, ServerMessageType message, string session, BinaryReader datareader, BinaryWriter datawriter)
        {
            var savemgr = backend.GetBackendComponent<SaveManager>();
            string key = datareader.ReadString();
            string val = datareader.ReadString();
            string ret = savemgr.GetValue(session, key, val);
            datawriter.Write(ret);
            return ServerResponseType.REQ_SUCCESS;
        }
    }

    /// <summary>
    /// Handler for setting save values
    /// </summary>
    [RequiresSession]
    public class SaveSetValueHandler : IMessageHandler
    {
        /// <inheritdoc/>
        public ServerMessageType HandledMessageType
        {
            get
            {
                return ServerMessageType.SAVE_SETVAL;
            }
        }

        /// <inheritdoc/>
        public ServerResponseType HandleMessage(Backend backend, ServerMessageType message, string session, BinaryReader datareader, BinaryWriter datawriter)
        {
            var savemgr = backend.GetBackendComponent<SaveManager>();
            string key = datareader.ReadString();
            string val = datareader.ReadString();
            savemgr.SetValue(session, key, val);
            return ServerResponseType.REQ_SUCCESS;
        }
    }

}
