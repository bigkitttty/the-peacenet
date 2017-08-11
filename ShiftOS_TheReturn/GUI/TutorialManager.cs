/* * Project: Plex *  * Copyright (c) 2017 Watercolor Games. All rights reserved. For internal use only. *  *  * The above copyright notice and this permission notice shall be included in all * copies or substantial portions of the Software. *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE * SOFTWARE. */using System;using System.Collections.Generic;using System.Linq;using System.Text;using System.Threading.Tasks;namespace Plex.Engine{    [Obsolete("This isn't used... I don't think...")]    public static class TutorialManager    {        /// <summary>        /// The tutorial frontend.        /// </summary>        private static ITutorial _tut = null;        /// <summary>        /// Registers a tutorial frontend to the backend.        /// </summary>        /// <param name="tut"></param>        public static void RegisterTutorial(ITutorial tut)        {            IsInTutorial = false;            _tut = tut;            _tut.OnComplete += (o, a) =>            {                SaveSystem.CurrentSave.StoryPosition = 2;                IsInTutorial = false;            };        }        public static bool IsInTutorial        {            get; private set;        }        public static int Progress        {            get            {                return _tut.TutorialProgress;            }        }        public static void StartTutorial()        {            IsInTutorial = true;            _tut.Start();        }    }    public interface ITutorial    {        int TutorialProgress { get; set; }        void Start();        event EventHandler OnComplete;    }    public class TutorialLockAttribute : Attribute    {        public TutorialLockAttribute(int progress)        {            Progress = progress;        }                public TutorialLockAttribute() : this(0)        {                    }        public int Progress { get; private set; }    }}