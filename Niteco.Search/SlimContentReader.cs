using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;

namespace Niteco.Search
{
    internal class SlimContentReader
    {
        private readonly IContentRepository contentRepository;
        private readonly LanguageSelectorFactory languageSelectorFactory;
        private readonly Stack<ContentReference> backlog = new Stack<ContentReference>();
        private readonly Queue<IContent> queue = new Queue<IContent>();
        private readonly System.Func<IContent, bool> traverseChildren;

        public IContent Current
        {
            get;
            private set;
        }

        public SlimContentReader(IContentRepository contentRepository, LanguageSelectorFactory languageSelectorFactory, ContentReference start)
            : this(contentRepository, languageSelectorFactory, start, (IContent c) => true)
        {
        }

        public SlimContentReader(IContentRepository contentRepository, LanguageSelectorFactory languageSelectorFactory, ContentReference start, System.Func<IContent, bool> traverseChildren)
        {
            this.contentRepository = contentRepository;
            this.languageSelectorFactory = languageSelectorFactory;
            this.backlog.Push(start);
            this.traverseChildren = traverseChildren;
        }

        public bool Next()
        {
            if (this.backlog.Count == 0 && this.queue.Count == 0)
            {
                return false;
            }
            if (this.queue.Count == 0)
            {
                bool flag = true;
                ContentReference contentLink = this.backlog.Pop();
                foreach (IContent current in this.contentRepository.GetLanguageBranches<IContent>(contentLink))
                {
                    flag &= this.traverseChildren(current);
                    this.queue.Enqueue(current);
                }
                if (flag)
                {
                    IContent[] array = this.contentRepository.GetChildren<IContent>(contentLink, this.languageSelectorFactory.MasterLanguage()).ToArray<IContent>();
                    for (int i = array.Length; i > 0; i--)
                    {
                        var item = new ContentReference(array[i - 1].ContentLink.ID, array[i - 1].ContentLink.ProviderName);
                        this.backlog.Push(item);
                    }
                }
            }
            this.Current = this.queue.Dequeue();
            return true;
        }
    }
}
