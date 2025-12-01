# Wizards Game - Hugo Website

This directory contains the Hugo static site for the Wizards Game project.

## Structure

```
docs/
├── hugo.toml          # Hugo configuration
├── content/           # Content pages
│   ├── _index.md     # Home page
│   ├── gameplay.md   # Gameplay tutorial
│   ├── architecture.md # System architecture
│   ├── technical.md  # Technical details
│   └── download.md   # Download page
├── static/           # Static assets
│   └── images/       # Architecture diagrams
└── .github/
    └── workflows/
        └── hugo.yml  # GitHub Pages deployment
```

## Local Development

### Prerequisites

Install Hugo (extended version):
- **Windows:** `choco install hugo-extended`
- **Mac:** `brew install hugo`
- **Linux:** Download from [Hugo releases](https://github.com/gohugoio/hugo/releases)

### Running Locally

```bash
cd docs
hugo server -D
```

Visit `http://localhost:1313` to view the site.

## Deployment

The site is automatically deployed to GitHub Pages when changes are pushed to the `main` branch.

### Manual Deployment

```bash
cd docs
hugo --gc --minify
```

The built site will be in the `public/` directory.

## Theme

This site uses the PaperMod theme. To install it:

```bash
cd docs
git submodule add --depth=1 https://github.com/adityatelange/hugo-PaperMod.git themes/PaperMod
git submodule update --init --recursive
```

## Content Updates

- Edit markdown files in `content/`
- Add images to `static/images/`
- Update configuration in `hugo.toml`

## Links

- [Live Site](https://joseph-ampfer.github.io/ninja_wizards_hands/)
- [Main Repository](https://github.com/joseph-ampfer/ninja_wizards_hands)

