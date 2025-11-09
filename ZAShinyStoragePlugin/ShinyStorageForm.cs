using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace ZAShinyStoragePlugin
{
    public partial class ShinyStorageForm : Form
    {
        private const int ENTRY_COUNT = 10;
        private const int ENTRY_SIZE = 0x1F0;
        private const int SIZE_9PARTY = 0x158;
        private const int POKEMON_DATA_OFFSET = 8;
        private static readonly byte[] StartEndMarker = [0x45, 0x26, 0x22, 0x84, 0xE4, 0x9C, 0xF2, 0xCB];

        private readonly List<PictureBox> StoredShinies;
        private readonly List<PA9?> CachedPokemon = [];
        private readonly SAV9ZA? SAV;
        private readonly SCBlock? ShinyBlock;
        private int SelectedSlot = -1;

        private readonly GameStrings Strings = GameInfo.Strings;

        public ShinyStorageForm(SAV9ZA sav9za)
        {
            InitializeComponent();
            SAV = sav9za;
            ShinyBlock = SAV.Accessor.GetBlock(SaveBlockAccessor9ZA.KStoredShinyEntity);
            StoredShinies = [StoredShiny1, StoredShiny2, StoredShiny3, StoredShiny4, StoredShiny5,
                           StoredShiny6, StoredShiny7, StoredShiny8, StoredShiny9, StoredShiny10];
            for (int i = 0; i < StoredShinies.Count; i++)
            {
                StoredShinies[i].Tag = i;
                StoredShinies[i].Cursor = Cursors.Hand;
            }
            var parent = FindForm();
            if (parent != null)
                this.CenterToForm(parent);
        }

        public void LoadPokemon()
        {
            CachedPokemon.Clear();

            for (int i = 0; i < ENTRY_COUNT; i++)
            {
                int entryOffset = i * ENTRY_SIZE + POKEMON_DATA_OFFSET;
                var slice = ShinyBlock!.Data.Slice(entryOffset, SIZE_9PARTY);

                PA9? pk = EntityDetection.IsPresent(slice)
                    ? new PA9(slice.ToArray())
                    : null;

                CachedPokemon.Add(pk);
            }

            PopulateStoredShinies();
            UpdateStatusLabel(CachedPokemon.Count(p => p != null));
            ClearDetailPanel();
        }

        private void PopulateStoredShinies()
        {
            for (int i = 0; i < ENTRY_COUNT; i++)
            {
                var pb = StoredShinies[i];
                var pk = CachedPokemon[i];

                if (pk != null)
                {
                    pb.Image = pk.Sprite();
                    pb.BackColor = Color.LightYellow;
                }
                else
                {
                    pb.Image = null;
                    pb.BackColor = Color.WhiteSmoke;
                }
            }
        }

        private void UpdateStatusLabel(int occupiedCount)
        {
            if (lbl_Status != null)
            {
                lbl_Status.Text = $"Stored Shinies: {occupiedCount} / {ENTRY_COUNT}";
                lbl_Status.ForeColor = occupiedCount > 0 ? Color.DarkGreen : Color.Gray;
            }
        }

        private void Slot_Click(object? sender, EventArgs e)
        {
            if (sender is not PictureBox pb || pb.Tag is not int slot)
                return;

            // Deselect previous slot
            if (SelectedSlot >= 0 && SelectedSlot < StoredShinies.Count)
            {
                var prevPk = CachedPokemon[SelectedSlot];
                StoredShinies[SelectedSlot].BackColor = prevPk != null ? Color.LightYellow : Color.WhiteSmoke;
            }

            // Select new slot
            SelectedSlot = slot;
            pb.BackColor = Color.LightBlue;

            var pokemon = CachedPokemon[slot];
            if (pokemon != null)
            {
                DisplayPokemonDetails(pokemon, slot);
                btn_RemoveSelected.Enabled = true;
            }
            else
            {
                ClearDetailPanel();
                btn_RemoveSelected.Enabled = false;
            }
        }

        private void DisplayPokemonDetails(PA9 pk, int slot)
        {
            if (lbl_Details == null) return;

            var species = Strings.Species[pk.Species];
            var formName = ShowdownParsing.GetStringFromForm(pk.Form, Strings, pk.Species, pk.Context);
            if (!string.IsNullOrEmpty(formName))
                species += $"-{formName}";
            var ability = Strings.Ability[pk.Ability];
            var genderStr = pk.Gender == 0 ? "♂ Male" : pk.Gender == 1 ? "♀ Female" : "Genderless";

            var details = $"{species}{Environment.NewLine}";
            details += $"Level: {pk.CurrentLevel}{Environment.NewLine}";
            details += $"Gender: {genderStr}{Environment.NewLine}";
            details += $"Nature: {pk.Nature}{Environment.NewLine}";
            details += $"Ability: {ability}{Environment.NewLine}";
            details += $"IVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}{Environment.NewLine}";

            for (int i = 0; i < 4; i++)
            {
                var move = pk.GetMove(i);
                if (move > 0)
                {
                    var moveName = Strings.Move[move];
                    details += $"- {moveName}{Environment.NewLine}";
                }
            }

            lbl_Details.Text = details;
        }

        private void ClearDetailPanel()
        {
            if (lbl_Details != null)
            {
                lbl_Details.Text = "Select a Pokemon to view details";
            }
        }

        private void Btn_RemoveSelected_Click(object? sender, EventArgs e)
        {
            if (SelectedSlot < 0)
                return;

            if (CachedPokemon[SelectedSlot] == null)
                return;

            var pokemon = CachedPokemon[SelectedSlot];
            var species = Strings.Species[pokemon!.Species];
            var result = MessageBox.Show(
                $"Are you sure you want to remove this shiny Pokemon from the cache?\n\n" +
                $"{species}\n" +
                $"Level: {pokemon.CurrentLevel}",
                "Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            ClearSlot(SelectedSlot);
            ApplyChanges(ShinyBlock!.Data);
            LoadPokemon();

            MessageBox.Show("Pokemon removed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Btn_RemoveAll_Click(object? sender, EventArgs e)
        {
            int occupiedCount = CachedPokemon.Count(pk => pk != null);

            if (occupiedCount == 0)
            {
                MessageBox.Show("There are no stored shiny Pokemon to clear.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to clear ALL {occupiedCount} stored shiny Pokemon?\n\n" +
                "This action cannot be undone!",
                "Confirm Clear All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            for (int i = 0; i < ENTRY_COUNT; i++)
                CachedPokemon[i] = null;

            ClearAllSlots();
            ApplyChanges(ShinyBlock!.Data);
            LoadPokemon();

            MessageBox.Show("All entries cleared successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearEntry(int index)
        {
            if (index < 0 || index >= ENTRY_COUNT)
                return;

            // Slice the entry
            var slice = ShinyBlock!.Data.Slice(index * ENTRY_SIZE, ENTRY_SIZE);

            // Format the entry
            slice.Clear();
            StartEndMarker.CopyTo(slice[..StartEndMarker.Length]);
            StartEndMarker.CopyTo(slice.Slice(ENTRY_SIZE - StartEndMarker.Length, StartEndMarker.Length));
            slice[424] = 0x01;

            // Clear cached Pokémon reference
            CachedPokemon[index] = null;
        }

        private void ClearSlot(int index)
        {
            if (index < 0 || index >= ENTRY_COUNT)
                return;

            // Shift subsequent entries up
            var data = ShinyBlock!.Data;
            for (int src = index + 1; src < ENTRY_COUNT; src++)
            {
                int srcStart = src * ENTRY_SIZE;
                int destStart = (src - 1) * ENTRY_SIZE;
                data.Slice(srcStart, ENTRY_SIZE).CopyTo(data.Slice(destStart, ENTRY_SIZE));

                CachedPokemon[src - 1] = CachedPokemon[src];
            }

            // Clear the previous slot after shifting
            ClearEntry(ENTRY_COUNT - 1);

            ApplyChanges(data);
            PopulateStoredShinies();
        }

        private void ClearAllSlots()
        {
            for (int i = 0; i < ENTRY_COUNT; i++)
                ClearEntry(i);

            ApplyChanges(ShinyBlock!.Data);
            PopulateStoredShinies();
        }

        private void ApplyChanges(Span<byte> data)
        {
            ShinyBlock!.ChangeData(data);
            SAV!.State.Edited = true;
        }
    }
}