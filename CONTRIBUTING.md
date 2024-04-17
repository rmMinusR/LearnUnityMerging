Repositories are designed to track different versions of projects, making it unwieldy to version the repository (and its branches). We could use a patch-based workflow, but that's not beginner-friendly. Here is how this repo stores previous versions of merge challenges:
tag: `challenge-name/v1/branch-name` (archive)
tag: `challenge-name/v2/branch-name` (archive)
tag: `challenge-name/v3/branch-name` (archive)
branch: `challenge-name/branch-name` (release, ready for the public - same commit as `challenge-name/v3/branch-name`)
branches: dev/\*, eg. dev/challenge-name/branch-name (development, not ready for the public)
