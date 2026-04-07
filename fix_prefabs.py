import re, os

prefab_dir = r'd:\Grudge\GRUDGE-NEW-GooglePlay\FRESH GRUDGE\Assets\uMMORPG\Bundle Configs\Prefabs\UmaCharacters\Entities\Players'
files = ['Elf_UMA.prefab', 'Human_UMA.prefab', 'Orc_UMA.prefab', 'Werewolf_UMA.prefab']

# Pattern: PrefabInstance block with missing GUID + 2 trailing stripped object blocks
pattern = re.compile(
    r'--- !u!1001 &7378143083721068523\r?\nPrefabInstance:.*?'
    r'--- !u!4 &7142558495042870501 stripped\r?\nTransform:.*?m_PrefabAsset: \{fileID: 0\}\r?\n',
    re.DOTALL
)

for fname in files:
    path = os.path.join(prefab_dir, fname)
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    before = len(content)
    new_content, n = pattern.subn('', content)
    if n:
        with open(path, 'w', encoding='utf-8', newline='') as f:
            f.write(new_content)
        print(f"{fname}: removed {before - len(new_content)} chars ({n} match)")
    else:
        print(f"{fname}: NO MATCH FOUND")
