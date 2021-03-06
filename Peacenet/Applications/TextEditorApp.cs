﻿using Plex.Engine;
using Plex.Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peacenet.CoreUtils;
using Microsoft.Xna.Framework;
using Peacenet.Filesystem;
using System.IO;

namespace Peacenet.Applications
{
    /// <summary>
    /// Provides a simple GUI-based text editor for the Peacenet.
    /// </summary>
    [AppLauncher("Text Editor", "Accessories", "Write, save, and open a text document")]
    public class TextEditorApp : Window, IFileViewer
    {
        [Dependency]
        private GUIUtils _guiutils = null;

        [Dependency]
        private FileUtils _futils = null;

        [Dependency]
        private FSManager _fs = null;

        [Dependency]
        private InfoboxManager _infobox = null;

        private string _path = null;

        private Button _open = new Button();
        private Button _save = new Button();
        private Button _new = new Button();
        private TextEditor _editor = new TextEditor();

        public string Text
        {
            get
            {
                return _editor.Text;
            }
            set
            {
                _editor.Text = value;
            }
        }

        public string FilePath => _path;

        /// <inheritdoc/>
        public TextEditorApp(WindowSystem _winsys) : base(_winsys)
        {
            Title = "New document - Text Editor";
            Width = 700;
            Height = 600;
            AddChild(_open);
            AddChild(_save);
            AddChild(_new);
            AddChild(_editor);


            _new.Click += (o, a) =>
            {
                _editor.Text = "";
                Title = "New document - Text Editor";
                _path = null;
            };
            _open.Click += (o, a) =>
            {
                _guiutils.AskForFile(false, (file) =>
                {
                    try
                    {
                        _editor.Text = _fs.ReadAllText(file);
                        Title = $"{_futils.GetNameFromPath(file)} - Text Editor";
                        _path = file;
                    }
                    catch (InvalidDataException)
                    {
                        _infobox.Show("Text Editor", $"'{file}' contains invalid characters and could not be opened.  Are you sure it's a text document?");
                    }
                });
            };
            _save.Click += (o, a) =>
            {
                _guiutils.AskForFile(true, (file) =>
                {
                    _fs.WriteAllText(file, _editor.Text);
                    Title = $"{_futils.GetNameFromPath(file)} - Text Editor";
                    _path = file;
                });
            };
        }

        /// <inheritdoc/>
        protected override void OnUpdate(GameTime time)
        {
            _new.Text = "New";
            _open.Text = "Open";
            _save.Text = "Save";
            _new.X = 3;
            _new.Y = 3;
            _open.Y = 3;
            _save.Y = 3;
            _open.X = _new.X + _new.Width + 3;
            _save.X = _open.X + _open.Width + 3;
            _editor.X = 0;
            _editor.Y = _save.Y + _save.Height + 3;
            _editor.Width = Width;
            _editor.Height = Height - _editor.Y;
        }

        public void View(string path)
        {
            _editor.Text = _fs.ReadAllText(path);
            _path = path;
        }
    }

    public class TextHandler : IFileHandler
    {
        public string Name => "Text Editor";

        public IEnumerable<string> MimeTypes
        {
            get
            {
                yield return "text/plain";
            }
        }

        [Dependency]
        private WindowSystem _winsys = null;

        [Dependency]
        private FSManager _fs = null;

        public void OpenFile(string path)
        {
            var editor = new TextEditorApp(_winsys);
            editor.View(path);
            editor.Show();
        }
    }
}