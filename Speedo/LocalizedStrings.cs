// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using Speedo.Languages;

namespace Speedo
{
    /// <summary>
    /// Provides access to string resources from XAML.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _resources = new AppResources();

        /// <summary>
        /// Gets the localized resources.
        /// </summary>
        public AppResources Resources
        {
            get { return _resources; }
        }
    }
}