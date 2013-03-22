// new

using System;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Windows.Phone.Speech.Synthesis;

namespace Speedo
{
    public sealed class SpeedAlert
    {
        public SpeedUnit Unit { get; set; }
        public int Limit { get; set; }
        public bool IsEnabled { get; set; }
        public INotificationProvider NotificationProvider { get; set; }

        private MovementSource source;
        private DispatcherTimer timer;

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

            source.ReadingChanged += Source_ReadingChanged;
        }

        private void Source_ReadingChanged( object sender, EventArgs e )
        {
            if ( IsEnabled && source.Speed > Limit && !timer.IsEnabled )
            {
                NotificationProvider.Notify();
                timer.Start();
            }
            else if ( timer.IsEnabled )
            {
                timer.Stop();
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
                var soundStream = TitleContainer.OpenStream( "Resources/alert.wav" );
                sound = SoundEffect.FromStream( soundStream );
            }

            public void Notify()
            {
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
                await synthesizer.SpeakTextAsync( "You're going too fast." );
            }
        }
    }
}