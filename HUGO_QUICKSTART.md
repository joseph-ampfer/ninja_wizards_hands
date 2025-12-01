# Hugo Website Quick Start Guide

Your Hugo website has been created in the `docs/` folder!

## 🚀 Next Steps

### 1. Install the Theme

Run this command from the repository root:

```bash
cd docs
git submodule add --depth=1 https://github.com/adityatelange/hugo-PaperMod.git themes/PaperMod
```

### 2. Test Locally (Optional)

If you have Hugo installed:

```bash
cd docs
hugo server -D
```

Visit: http://localhost:1313

### 3. Push to GitHub

```bash
git add .
git commit -m "Add Hugo website for GitHub Pages"
git push
```

### 4. Enable GitHub Pages

1. Go to your repository on GitHub
2. Click **Settings** → **Pages**
3. Under **Source**, select **"GitHub Actions"**
4. Wait a few minutes for the deployment

Your site will be live at:
**https://joseph-ampfer.github.io/ninja_wizards_hands/**

## 📁 What Was Created

```
docs/
├── hugo.toml              # Hugo configuration
├── content/
│   ├── _index.md         # Home page
│   ├── gameplay.md       # Gameplay tutorial
│   ├── architecture.md   # Architecture documentation
│   ├── technical.md      # Technical details
│   └── download.md       # Download page
├── static/
│   └── images/           # All architecture diagrams (copied)
└── .github/
    └── workflows/
        └── hugo.yml      # Auto-deployment workflow
```

## 📝 Content Pages

Your website includes:
- **Home** - Project overview and quick links
- **Gameplay Tutorial** - How to play guide with spell lists
- **Architecture** - System design and patterns
- **Technical Details** - Implementation deep dive
- **Download** - Demo download and setup

## 🎨 Theme

The site uses **PaperMod** - a modern, fast Hugo theme.

After installing the theme (step 1 above), the site will have:
- Clean, professional design
- Responsive mobile layout
- Dark mode support
- Fast page loads
- SEO optimized

## 🔧 Customization

### Update Content
Edit markdown files in `docs/content/`

### Add Images
Place files in `docs/static/images/`

### Change Settings
Edit `docs/hugo.toml`

### Add Pages
1. Create `docs/content/newpage.md`
2. Add menu entry in `hugo.toml`

## 📖 Documentation Links

For detailed setup instructions, see:
- `docs/README.md` - Overview of the Hugo structure
- `docs/SETUP.md` - Detailed setup and troubleshooting

## ❓ Troubleshooting

### Theme Not Found
```bash
cd docs
git submodule update --init --recursive
```

### Need Hugo?
- Windows: `choco install hugo-extended`
- Mac: `brew install hugo`
- Linux: `sudo snap install hugo`

### Images Not Showing Locally?
Images are in `docs/static/images/` and referenced as `![](../images/file.png)`

## ✅ What to Document

For your project submission, update these addresses:

```
- GitHub address: https://github.com/joseph-ampfer/ninja_wizards_hands/tree/main
- Code address: https://github.com/joseph-ampfer/ninja_wizards_hands/tree/main/code/Scripts
- Test address: https://github.com/joseph-ampfer/ninja_wizards_hands/tree/main/code/Tests
- Doc address: https://joseph-ampfer.github.io/ninja_wizards_hands/
```

Or point to the README:
```
- Doc address (README): https://github.com/joseph-ampfer/ninja_wizards_hands/blob/main/README.md
```

Both are valid - the Hugo site is for presentation, README is comprehensive documentation.

---

**Ready to go live!** Follow steps 1-4 above to deploy your site.

