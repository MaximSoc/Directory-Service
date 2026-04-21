using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel
{
    public class Errors : IEnumerable<Error>
    {
        public List<Error> Items { get; init; } = new();

        public Errors() { }

        public Errors(IEnumerable<Error> errors)
        {
            Items = errors.ToList();
        }

        public IEnumerator<Error> GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator Errors(List<Error> errors) => new(errors);
        public static implicit operator Errors(Error error) => new([error]);
    }
}
