#!/usr/bin/env python3
"""
Static UPM Scoped Registry Generator for ETPlugin.
Builds an npm-compatible static registry (packuments, /-/v1/search, /-/all, and .tgz tarballs)
that can be hosted directly on GitHub Pages or any static file server for Unity Package Manager.
"""

from __future__ import annotations

import argparse
import base64
import datetime
import gzip
import hashlib
import io
import json
import os
import shutil
import tarfile
from pathlib import Path


EXCLUDED_NAMES = {".DS_Store", "Thumbs.db", ".git", ".gitignore"}


def create_deterministic_tgz(package_dir: Path, output_tgz: Path) -> tuple[str, str]:
    """
    Packs a Unity package directory into an npm-compatible .tgz archive where:
    - All entries are prefixed with 'package/'
    - 'package/package.json' is the very first entry (required by UnityPackageManager)
    - All .meta files are preserved so Unity GUIDs never break
    Returns (shasum_sha1_hex, integrity_sha512_sri).
    """
    output_tgz.parent.mkdir(parents=True, exist_ok=True)

    all_files: list[Path] = []
    for root, dirs, files in os.walk(package_dir):
        dirs[:] = sorted(d for d in dirs if d not in EXCLUDED_NAMES)
        for fname in sorted(files):
            if fname in EXCLUDED_NAMES:
                continue
            fpath = Path(root) / fname
            all_files.append(fpath)

    # Ensure package.json is first entry in the tar archive
    pkg_json_path = package_dir / "package.json"
    all_files = [pkg_json_path] + [p for p in all_files if p != pkg_json_path]

    tar_buffer = io.BytesIO()
    with tarfile.open(fileobj=tar_buffer, mode="w", format=tarfile.USTAR_FORMAT) as tar:
        for fpath in all_files:
            rel_path = fpath.relative_to(package_dir).as_posix()
            arcname = f"package/{rel_path}"
            data = fpath.read_bytes()

            info = tarfile.TarInfo(name=arcname)
            info.size = len(data)
            info.mtime = 0
            info.mode = 0o644
            info.type = tarfile.REGTYPE
            info.uid = 0
            info.gid = 0
            info.uname = "root"
            info.gname = "root"
            tar.addfile(info, io.BytesIO(data))

    raw_tar_bytes = tar_buffer.getvalue()
    with open(output_tgz, "wb") as f_out:
        with gzip.GzipFile(filename="", mode="wb", fileobj=f_out, mtime=0) as gz:
            gz.write(raw_tar_bytes)

    tgz_bytes = output_tgz.read_bytes()
    shasum = hashlib.sha1(tgz_bytes).hexdigest()
    sha512_b64 = base64.b64encode(hashlib.sha512(tgz_bytes).digest()).decode("ascii")
    integrity = f"sha512-{sha512_b64}"
    return shasum, integrity


def build_registry(
    packages_root: Path,
    output_dir: Path,
    registry_url: str,
    existing_dir: Path | None = None,
    tarball_base_url: str | None = None,
) -> None:
    registry_url = registry_url.rstrip("/")
    if not tarball_base_url:
        if "tonicatfelidae.github.io/ETPlugin" in registry_url:
            tarball_base_url = "https://raw.githubusercontent.com/TonicatFelidae/ETPlugin/gh-pages/tarballs"
        else:
            tarball_base_url = f"{registry_url}/tarballs"
    tarball_base_url = tarball_base_url.rstrip("/")

    now_iso = datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.000Z")
    now_ms = int(datetime.datetime.now(datetime.timezone.utc).timestamp() * 1000)

    if output_dir.exists():
        shutil.rmtree(output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    tarballs_dir = output_dir / "tarballs"
    tarballs_dir.mkdir(parents=True, exist_ok=True)

    # Preserve existing tarballs if provided
    if existing_dir and (existing_dir / "tarballs").is_dir():
        for existing_tgz in (existing_dir / "tarballs").glob("*.tgz"):
            shutil.copy2(existing_tgz, tarballs_dir / existing_tgz.name)

    search_objects = []
    all_packages_index = {"_updated": now_ms}
    package_summaries = []

    for pkg_dir in sorted(packages_root.iterdir()):
        if not pkg_dir.is_dir():
            continue
        pkg_json_file = pkg_dir / "package.json"
        if not pkg_json_file.is_file():
            continue

        manifest = json.loads(pkg_json_file.read_text(encoding="utf-8"))
        name = manifest["name"]
        version = manifest["version"]
        display_name = manifest.get("displayName", name)
        description = manifest.get("description", "")
        author = manifest.get("author", {"name": "Nekoniii Studio"})
        keywords = manifest.get("keywords", ["et", "nekoniiistudio", "unity"])

        readme_text = ""
        for readme_candidate in ["README.md", "Readme.md", "readme.md"]:
            rpath = pkg_dir / readme_candidate
            if rpath.is_file():
                readme_text = rpath.read_text(encoding="utf-8")
                break

        tgz_filename = f"{name}-{version}.tgz"
        tgz_path = tarballs_dir / tgz_filename
        shasum, integrity = create_deterministic_tgz(pkg_dir, tgz_path)
        tarball_url = f"{tarball_base_url}/{tgz_filename}"

        # Load previous packument if available to retain older versions
        versions_map = {}
        time_map = {"created": now_iso, "modified": now_iso}
        if existing_dir and (existing_dir / name).is_file():
            try:
                old_packument = json.loads((existing_dir / name).read_text(encoding="utf-8"))
                versions_map = old_packument.get("versions", {})
                old_time = old_packument.get("time", {})
                if "created" in old_time:
                    time_map["created"] = old_time["created"]
                for k, v in old_time.items():
                    if k not in ("created", "modified"):
                        time_map[k] = v
            except Exception:
                pass

        version_entry = dict(manifest)
        version_entry["_id"] = f"{name}@{version}"
        version_entry["readmeFilename"] = "README.md"
        if "dependencies" not in version_entry:
            version_entry["dependencies"] = {}
        version_entry["dist"] = {
            "shasum": shasum,
            "integrity": integrity,
            "tarball": tarball_url,
        }

        versions_map[version] = version_entry
        time_map[version] = now_iso
        time_map["modified"] = now_iso

        packument = {
            "_id": name,
            "name": name,
            "displayName": display_name,
            "description": description,
            "dist-tags": {
                "latest": version,
            },
            "versions": versions_map,
            "time": time_map,
            "author": author,
            "readme": readme_text,
            "_attachments": {},
        }

        # Write packument file at /<package-name>
        (output_dir / name).write_text(json.dumps(packument, indent=2), encoding="utf-8")

        # Add to /-/v1/search objects
        search_objects.append({
            "package": {
                "name": name,
                "scope": "unscoped",
                "version": version,
                "displayName": display_name,
                "description": description,
                "keywords": keywords,
                "date": now_iso,
                "author": author,
                "publisher": author,
                "maintainers": [author],
                "links": {
                    "npm": f"{registry_url}/{name}",
                    "homepage": "https://github.com/TonicatFelidae/ETPlugin",
                    "repository": "https://github.com/TonicatFelidae/ETPlugin.git",
                },
            },
            "score": {
                "final": 1.0,
                "detail": {
                    "quality": 1.0,
                    "popularity": 1.0,
                    "maintenance": 1.0,
                },
            },
            "searchScore": 1.0,
        })

        # Add to /-/all index
        all_versions_tags = {v: ("latest" if v == version else v) for v in versions_map.keys()}
        all_packages_index[name] = {
            "name": name,
            "displayName": display_name,
            "description": description,
            "author": author,
            "dist-tags": {"latest": version},
            "versions": all_versions_tags,
            "time": time_map,
        }

        package_summaries.append({
            "name": name,
            "displayName": display_name,
            "version": version,
            "description": description,
            "tarball": tarball_url,
        })
        print(f"Packed {name}@{version} -> {tgz_filename} (sha1={shasum[:10]}...)")

    # Write /-/all and /-/v1/search
    dash_dir = output_dir / "-"
    v1_dir = dash_dir / "v1"
    v1_dir.mkdir(parents=True, exist_ok=True)

    (dash_dir / "all").write_text(json.dumps(all_packages_index, indent=2), encoding="utf-8")
    search_payload = {
        "objects": search_objects,
        "total": len(search_objects),
        "time": now_iso,
    }
    (v1_dir / "search").write_text(json.dumps(search_payload, indent=2), encoding="utf-8")

    # Write .nojekyll so GitHub Pages serves '-' directories and extensionless files verbatim
    (output_dir / ".nojekyll").write_text("", encoding="utf-8")

    # Write a clean HTML landing page at index.html
    rows_html = "\n".join(
        f"<tr><td><code>{p['name']}</code></td><td><strong>{p['displayName']}</strong></td>"
        f"<td><code>{p['version']}</code></td><td>{p['description']}</td>"
        f"<td><a href=\"{p['tarball']}\">.tgz</a></td></tr>"
        for p in package_summaries
    )
    index_html = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>ET Plugin Scoped Registry</title>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; max-width: 960px; margin: 40px auto; padding: 0 20px; color: #222; line-height: 1.5; }}
    pre {{ background: #1e1e1e; color: #d4d4d4; padding: 16px; border-radius: 8px; overflow-x: auto; }}
    code {{ background: #f2f2f2; padding: 2px 6px; border-radius: 4px; }}
    pre code {{ background: none; padding: 0; }}
    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
    th, td {{ border-bottom: 1px solid #e0e0e0; text-align: left; padding: 10px 8px; font-size: 14px; }}
    th {{ background: #f8f9fa; }}
  </style>
</head>
<body>
  <h1>ET Plugin UPM Scoped Registry</h1>
  <p>Add this Scoped Registry to your Unity project's <code>Packages/manifest.json</code> to browse and install all ET packages directly inside <strong>Window &rarr; Package Manager &rarr; My Registries</strong>:</p>
  <pre><code>"scopedRegistries": [
  {{
    "name": "ET Plugin Registry",
    "url": "{registry_url}",
    "scopes": [
      "com.nekoniiistudio.et"
    ]
  }}
]</code></pre>
  <h2>Available Packages ({len(package_summaries)})</h2>
  <table>
    <thead>
      <tr><th>Package ID</th><th>Display Name</th><th>Latest</th><th>Description</th><th>Download</th></tr>
    </thead>
    <tbody>
      {rows_html}
    </tbody>
  </table>
</body>
</html>
"""
    (output_dir / "index.html").write_text(index_html, encoding="utf-8")
    print(f"\nRegistry successfully generated at: {output_dir} ({len(package_summaries)} packages)")


def main() -> None:
    parser = argparse.ArgumentParser(description="Build static UPM Scoped Registry for ETPlugin")
    parser.add_argument("--packages-root", default="Packages", help="Path to Packages directory")
    parser.add_argument("--output-dir", default="dist-registry", help="Output directory for static registry")
    parser.add_argument(
        "--registry-url",
        default="https://tonicatfelidae.github.io/ETPlugin",
        help="Public base URL of the hosted registry",
    )
    parser.add_argument("--existing-dir", default=None, help="Optional existing registry directory to preserve old versions")
    args = parser.parse_args()

    build_registry(
        packages_root=Path(args.packages_root).resolve(),
        output_dir=Path(args.output_dir).resolve(),
        registry_url=args.registry_url,
        existing_dir=Path(args.existing_dir).resolve() if args.existing_dir else None,
    )


if __name__ == "__main__":
    main()
