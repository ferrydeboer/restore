using System;
using JetBrains.Annotations;
using Restore.ChangeResolution;

namespace Restore.Tests.ChangeResolution
{
    public class TestResourceTranslator : ITranslator<LocalTestResource, RemoteTestResource>
    {
        public void TranslateForward([NotNull] LocalTestResource source, ref RemoteTestResource target)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            if (target == null)
            {
                target = new RemoteTestResource(source.Name);
            }
        }

        public void TranslateBackward([NotNull] RemoteTestResource source, ref LocalTestResource target)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            var idRandomizer = new Random(DateTime.Now.Millisecond);
            if (target == null)
            {
                target = new LocalTestResource(source.Id, idRandomizer.Next());
            }

            target.Name = source.Name;
        }
    }
}
