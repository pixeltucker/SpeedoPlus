// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Speedo.Languages;
using Windows.Phone.Speech.Synthesis;

namespace Speedo
{
    public sealed class SpeedAlert
    {
        private readonly MovementSource source;
        private readonly DispatcherTimer timer;

        public INotificationProvider NotificationProvider { get; set; }

        public static INotificationProvider SoundProvider { get; private set; }
        public static INotificationProvider SpeechProvider { get; private set; }

        static SpeedAlert()
        {
            SoundProvider = new SoundNotificationProvider();
            SpeechProvider = new SpeechNotificationProvider();
        }

        public SpeedAlert( MovementSource source, INotificationProvider provider )
        {
            this.source = source;
            NotificationProvider = provider;

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 5 ) };
            timer.Tick += Timer_Tick;

            source.PropertyChanged += Source_PropertyChanged;
        }

        private void Source_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == "Speed" )
            {
                if ( AppSettings.Current.IsSpeedAlertEnabled
                  && source.Speed > AppSettings.Current.SpeedLimit
                  && !timer.IsEnabled )
                {
                    NotificationProvider.Notify();
                    timer.Start();
                }
                else if ( timer.IsEnabled )
                {
                    timer.Stop();
                }
            }
        }

        private void Timer_Tick( object sender, EventArgs e )
        {
            NotificationProvider.Notify();
        }

        public interface INotificationProvider
        {
            void Notify();
        }

        private sealed class SoundNotificationProvider : INotificationProvider
        {
            private SoundEffect sound;

            public SoundNotificationProvider()
            {
                using ( var soundStream = TitleContainer.OpenStream( "Resources/alert.wav" ) )
                {
                    sound = SoundEffect.FromStream( soundStream );
                }
            }

            public void Notify()
            {
                FrameworkDispatcher.Update(); // needed for whatever reason
                sound.Play();
            }
        }

        private sealed class SpeechNotificationProvider : INotificationProvider
        {
            private SpeechSynthesizer synthesizer;

            public SpeechNotificationProvider()
            {
                synthesizer = new SpeechSynthesizer();
            }

            public async void Notify()
            {
                await synthesizer.SpeakTextAsync( AppResources.SpeechAlert );
            }
        }
    }
}