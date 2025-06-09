using IndieArtMarketplace.DAL;
using IndieArtMarketplace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndieArtMarketplace.Business.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<User> CreateUser(User user)
        {
            try
            {
                user.RegistrationDate = DateTime.UtcNow; // Ensure UTC time
                user.UserID = 0; // Reset UserID to let the database generate it
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
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

        public async Task<Artwork> CreateArtwork(Artwork artwork)
        {
            try
            {
                // Ensure UTC time
                artwork.UploadDate = DateTime.UtcNow;
                
                _context.Artworks.Add(artwork);
                await _context.SaveChangesAsync();
                return artwork;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating artwork");
                throw;
            }
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

        public async Task<MusicTrack> CreateMusicTrack(MusicTrack musicTrack)
        {
            try
            {
                // Ensure UTC time
                musicTrack.UploadDate = DateTime.UtcNow;
                
                _context.MusicTracks.Add(musicTrack);
                await _context.SaveChangesAsync();
                return musicTrack;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating music track");
                throw;
            }
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

        public async Task<Transaction> CreateTransaction(Transaction transaction)
        {
            try
            {
                transaction.PurchaseDate = DateTime.UtcNow; // Ensure UTC time
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                throw;
            }
        }
    }
}