using System;
using System.Collections.Generic;
using System.Linq;

namespace StansAssets.SceneManagement
{
    sealed class GroupRequest : Request
    {
        readonly List<Request> m_Requests;
        readonly int m_FinalCount;

        public override bool IsDone => SumProgress == m_FinalCount;

        public GroupRequest(int count) : base()
        {
            m_FinalCount = count;

            m_Requests = new List<Request>();
        }

        public override void UpdateProgress(float v)
        {
            throw new NotSupportedException();
        }

        public void AddRequest(Request r)
        {
            m_Requests.Add(r);

            r.ProgressChange += _ =>
            {
                SetProgress(SumProgress / m_FinalCount);
            };
            r.Done += TryDone;
        }

        void TryDone()
        {
            if (!IsDone)
                return;

            InvokeDone();
        }

        float SumProgress => m_Requests.Sum(r => r.Progress);
    }
}
