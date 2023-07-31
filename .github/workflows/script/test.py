import requests
import subprocess

print("im here")
url = f"https://raw.githubusercontent.com/Azure/azure-powershell/main/src/Compute/Compute/Extension/CustomScript/SetAzureVMCustomScriptExtensionCommand.cs"
response = requests.get(url)
if response.status_code == 200:
  content = response.content
  content_str = content.decode('utf-8')
  print(content_str)

code = content_str.split('public override void ExecuteCmdlet()')[1]

out = subprocess.run(['gh', 'issue', 'view','-R', 'Azure/azure-powershell-cmdlet-review-pr', '1352'], capture_output=True)

# parse stdout to get the issue body
issue_body = out.stdout.decode('utf-8').split('---')[0]

# find the string 'Sample of end-to-end usage' and then the immediate next line
sample_code = issue_body.split('### Changed cmdlet')[1]

# find the string '## Specific test cases' and then the previous line
test_case = sample_code.split('##')[0]

print(test_case)
