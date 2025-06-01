using IndieArtMarketplace.DAL;
using IndieArtMarketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace IndieArtMarketplace.Business.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // User methods
        public User GetUserByEmail(string email)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == email);
        }

        public User GetUserByEmailOrUsername(string input)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == input || u.Username == input);
        }

        public void CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users
                .AsNoTracking()
                .ToList();
        }

        // Artwork methods
        public IEnumerable<Artwork> GetAllArtworks()
        {
            return _context.Artworks
                .AsNoTracking()
                .ToList();
        }

        public Artwork GetArtworkById(int id)
        {
            return _context.Artworks
                .AsNoTracking()
                .FirstOrDefault(a => a.ArtworkID == id);
        }

        public void CreateArtwork(Artwork artwork)
        {
            _context.Artworks.Add(artwork);
            _context.SaveChanges();
        }

        // Music Track methods
        public IEnumerable<MusicTrack> GetAllMusicTracks()
        {
            return _context.MusicTracks
                .AsNoTracking()
                .ToList();
        }

        public MusicTrack GetMusicTrackById(int id)
        {
            return _context.MusicTracks
                .AsNoTracking()
                .FirstOrDefault(m => m.TrackID == id);
        }

        public void CreateMusicTrack(MusicTrack musicTrack)
        {
            _context.MusicTracks.Add(musicTrack);
            _context.SaveChanges();
        }

        // Transaction methods
        public IEnumerable<Transaction> GetAllTransactions()
        {
            return _context.Transactions
                .AsNoTracking()
                .ToList();
        }

        public Transaction GetTransactionById(int id)
        {
            return _context.Transactions
                .AsNoTracking()
                .FirstOrDefault(t => t.TransactionID == id);
        }

        public void CreateTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
        }
    }
}