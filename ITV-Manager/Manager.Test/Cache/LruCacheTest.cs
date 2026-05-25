using FluentAssertions;
using Manager.Cache;
using Manager.Config;

namespace Manager.Test.Cache;

[TestFixture]
[TestOf(typeof(LruCache<,>))]
public class LruCacheTest {
    
    private LruCache<int, string> _cache = null!;

    [SetUp]
    public void SetUp() {
        _cache = new LruCache<int, string>(AppConfig.CacheSize);
    }
    
    [Test]
    public void Constructor_ConCapacidadInvalida_DeberiaLanzarArgumentException() {
        // arrange + act
        Action actZero = () => new LruCache<int, string>(0);

        actZero.Should().Throw<ArgumentException>()
            .WithMessage("La capacidad de la caché debe ser mayor que 0.*")
            .And.ParamName.Should().Be("capacity");
    }
    
    [Test]
    public void DisplayStatus_DeberiaEjecutarseSinLanzarExcepciones()
    {
        // arrange
        var cache = new LruCache<int, string>(2);
        cache.Add(1, "Uno");
        cache.Add(2, "Dos");

        // act
        var act = () => cache.DisplayStatus();

        // assert
        act.Should().NotThrow();
    }

    [TestFixture] public class CasosAdd : LruCacheTest {
        
        [Test]
        public void Add_ConElementoValido_DeberiaGuardarElemento() {

            // act
            _cache.Add(1, "uno");

            // assert
            _cache.Get(1).Should().Be("uno");
        }
        
        [Test]
        public void Add_ClaveExistente_DeberiaActualizarValorYRejuvenecer() {
            
            var cache = new LruCache<int, string>(2); // creamos otra cache para no tener que hacer un add de 11 cosas
            
            // act
            cache.Add(1, "uno");
            cache.Add(2, "dos");
            cache.Add(1, "uno editado");
            cache.Add(3, "tres");
            
            // assert
            cache.Get(1).Should().Be("uno editado");
            cache.Get(2).Should().BeNull();
            cache.Get(3).Should().Be("tres");
        }

        [Test]
        public void Add_CacheLlena_DeberiaDesalojarElElementoMenosUsado() {
            
            var cache = new LruCache<int, string>(3); // creamos otra cache para no tener que hacer un add de 11 cosas
            
            // act
            cache.Add(1, "uno");
            cache.Add(2, "dos");
            cache.Add(3, "tres");
            cache.Add(4, "cuatro");

            // assert
            cache.Get(1).Should().BeNull();
            cache.Get(2).Should().Be("dos");
            cache.Get(3).Should().Be("tres");
            cache.Get(4).Should().Be("cuatro");
        }
    }

    [TestFixture] public class CasosGet : LruCacheTest {
        
        [Test]
        public void Get_ClaveInexistente_DeberiaRetornarDefault() {
            // act
            var resultado = _cache.Get(AppConfig.CacheSize + 10);

            // assert
            resultado.Should().BeNull();
        }
        
        [Test]
        public void Get_ClaveExistente_DeberiaRejuvenecerLaPrioridad() {
            // arrange
            var cache = new LruCache<int, string>(3);
            cache.Add(1, "Uno");
            cache.Add(2, "Dos");
            cache.Add(3, "Tres");

            // act
            cache.Get(1).Should().Be("Uno");
            cache.Add(4, "Cuatro");

            // assert
            cache.Get(2).Should().BeNull(); 
            cache.Get(1).Should().Be("Uno"); 
            cache.Get(3).Should().Be("Tres");
            cache.Get(4).Should().Be("Cuatro");
        }
    }

    [TestFixture] public class CasosRemove : LruCacheTest {
        [Test]
        public void Remove_ClaveInexistente_DeberiaDevolverFalse() {
            
            // arrange
            var resultado = _cache.Remove(99);

            // assert
            resultado.Should().BeFalse();
        }
        
        [Test]
        public void Remove_ClaveExistente_DeberiaDevolverTrue() {
            
            _cache.Add(1, "UNO");
            // arrange
            var resultado = _cache.Remove(1);

            // assert
            resultado.Should().BeTrue();
        }
        
        [Test]
        public void Clear_DeberiaDevolverTrue() {
            
            _cache.Add(1, "uno");
            _cache.Get(1).Should().Be("uno");
            
            _cache.Clear();
            _cache.Get(1).Should().BeNull();
        }
    }
}