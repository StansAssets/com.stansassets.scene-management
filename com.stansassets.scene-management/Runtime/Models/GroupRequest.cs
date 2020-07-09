using System;
using System.Collections.Generic;
using System.Linq;

namespace StansAssets.SceneManagement
{
    public sealed class GroupRequest : Request
    {
        readonly List<Request> m_requests;
        readonly int m_finalCount;

        public override bool IsDone => SumProgress == m_finalCount;

        public GroupRequest(int count) : base()
        {
            m_finalCount = count;

            m_requests = new List<Request>();
        }

        public override void UpdateProgress(float v)
        {
            throw new NotSupportedException();
        }

        public void AddRequest(Request r)
        {
            m_requests.Add(r);

            r.ProgressChange += _ =>
            {
                SetProgress(SumProgress / m_finalCount);
            };
            r.Done += TryDone;
        }

        void TryDone()
        {
            if (!IsDone)
                return;

            InvokeDone();
        }

        float SumProgress => m_requests.Sum(r => r.Progress);
    }
}
