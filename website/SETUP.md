# Hugo Website Setup Instructions

## Quick Start

### 1. Install Hugo

**Windows:**
```bash
choco install hugo-extended
```

**macOS:**
```bash
brew install hugo
```

**Linux:**
```bash
sudo snap install hugo
```

Or download from: https://github.com/gohugoio/hugo/releases

### 2. Install the Theme

From the repository root:

```bash
cd docs
git submodule add --depth=1 https://github.com/adityatelange/hugo-PaperMod.git themes/PaperMod
git submodule update --init --recursive
```

### 3. Run Locally

```bash
cd docs
hugo server -D
```

Visit: http://localhost:1313

### 4. Build for Production

```bash
cd docs
hugo --gc --minify
```

Output will be in `docs/public/`

## GitHub Pages Setup

### Configure Repository Settings

1. Go to repository **Settings** → **Pages**
2. Under **Source**, select "GitHub Actions"
3. The workflow file at `.github/workflows/hugo.yml` will handle deployment

### Update Base URL

In `hugo.toml`, ensure the baseURL is correct:

```toml
baseURL = 'https://joseph-ampfer.github.io/ninja_wizards_hands/'
```

### Deploy

Simply push to the `main` branch:

```bash
git add .
git commit -m "Add Hugo website"
git push
```

The GitHub Action will automatically build and deploy the site.

## Troubleshooting

### Theme Not Found

If you see "theme not found" error:

```bash
cd docs
git submodule update --init --recursive
```

### Build Errors

Ensure you're using Hugo extended version:

```bash
hugo version
```

Should show "hugo v0.xxx.x+extended"

### Images Not Showing

- Images must be in `docs/static/images/`
- Reference them in markdown as: `![Alt text](../images/filename.png)`

### Local Development vs Production

When running locally with `hugo server`, the baseURL is ignored.
In production (GitHub Pages), ensure baseURL matches your GitHub Pages URL.

## Content Management

### Adding a New Page

1. Create a new markdown file in `docs/content/`
2. Add front matter:

```yaml
---
title: "Page Title"
weight: 60
---
```

3. Add to navigation menu in `hugo.toml`:

```toml
[[menu.main]]
identifier = "newpage"
name = "New Page"
url = "/newpage/"
weight = 60
```

### Updating Existing Content

Simply edit the markdown files in `docs/content/` and save.

## Theme Customization

To customize the PaperMod theme:

1. Create `docs/layouts/` directory
2. Copy template files from theme
3. Modify as needed

See: https://github.com/adityatelange/hugo-PaperMod/wiki

## Support

- [Hugo Documentation](https://gohugo.io/documentation/)
- [PaperMod Wiki](https://github.com/adityatelange/hugo-PaperMod/wiki)
- [GitHub Pages Documentation](https://docs.github.com/en/pages)

