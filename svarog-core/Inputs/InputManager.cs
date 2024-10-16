using SFML.System;

namespace svarog_core.Inputs
{
    public class InputManager<TKey> where TKey : notnull
    {
        private HashSet<TKey> LongDown = [];
        private readonly Dictionary<TKey, DateTime> LongDownDuration = [];
        private HashSet<TKey> CurrentlyDown = [];
        private HashSet<TKey> CurrentlyUp = [];

        public double GetHoldDuration(TKey index)
        {
            if (LongDownDuration.TryGetValue(index, out DateTime start))
            {
                return (DateTime.Now - start).TotalMilliseconds;
            }
            else
            {
                return 0.0;
            }
        }

        public bool IsDown(TKey index) => LongDown.Contains(index);
        public bool IsJustPressed(TKey index) => CurrentlyDown.Contains(index);
        public bool IsJustReleased(TKey index) => CurrentlyUp.Contains(index);

        public IEnumerable<TKey> GetAllDown()
        {
            foreach (var input in LongDown)
            {
                yield return input;
            }
        }

        public IEnumerable<TKey> GetAllJustPressed()
        {
            foreach (var input in CurrentlyDown)
            {
                yield return input;
            }
        }

        public IEnumerable<TKey> GetAllJustReleased()
        {
            foreach (var input in CurrentlyUp)
            {
                yield return input;
            }
        }

        public void InputDown(TKey input)
        {
            CurrentlyDown.Add(input);
            LongDownDuration.Add(input, DateTime.Now);
            LongDown.Add(input);
        }

        public void InputUp(TKey input)
        {
            CurrentlyUp.Add(input);
            LongDownDuration.Remove(input);
            LongDown.Remove(input);
        }

        public void Frame()
        {
            CurrentlyDown.Clear();
            CurrentlyUp.Clear();
        }
    }
}
