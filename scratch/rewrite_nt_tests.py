import re

with open('Backend.ApiTests/NT_TemplateManagementTests.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Add Backend.Helpers
if 'using Backend.Helpers;' not in content:
    content = content.replace('using Backend.Models;', 'using Backend.Helpers;\nusing Backend.Models;')

# Fix GetSharedTestConnectionString warning
content = content.replace('private string GetSharedTestConnectionString()', 'private new string GetSharedTestConnectionString()')
content = content.replace('private string GetSharedTestPassword()', 'private new string GetSharedTestPassword()')

# Fix CreateClientAsync
content = content.replace('var client = Factory.CreateClient();', 'var client = new HttpClient { BaseAddress = BaseUri };')

# Fix SetupTestEnvAsync
setup_env_old = '''    private async Task<(int campusId, HttpClient superAdmin, HttpClient campusAdmin, HttpClient student)> SetupTestEnvAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();'''
setup_env_new = '''    private async Task<(int campusId, HttpClient superAdmin, HttpClient campusAdmin, HttpClient student)> SetupTestEnvAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetSharedTestConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);'''
content = content.replace(setup_env_old, setup_env_new)

# Fix AdminDeactivatesTemplateWithoutDeleting
admin_deact_old = '''        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();'''
admin_deact_new = '''        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetSharedTestConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);'''
content = content.replace(admin_deact_old, admin_deact_new)

# Fix possible null references
content = content.replace('Assert.That(data?.Data, Is.Not.Null);', 'Assert.That(data!.Data, Is.Not.Null);')
content = content.replace('var tplId = data.Data.MaMauThongBao;', 'var tplId = data!.Data!.MaMauThongBao;')
content = content.replace('Assert.That(previewData?.Data, Is.Not.Null);', 'Assert.That(previewData!.Data, Is.Not.Null);')
content = content.replace('Assert.That(previewData.Data.RenderedTitle', 'Assert.That(previewData!.Data!.RenderedTitle')
content = content.replace('Assert.That(previewData.Data.RenderedBody', 'Assert.That(previewData!.Data!.RenderedBody')
content = content.replace('Assert.That(previewData.Data.MissingVariables', 'Assert.That(previewData!.Data!.MissingVariables')
content = content.replace('Assert.That(previewData.Data.DetectedPlaceholders', 'Assert.That(previewData!.Data!.DetectedPlaceholders')

with open('Backend.ApiTests/NT_TemplateManagementTests.cs', 'w', encoding='utf-8') as f:
    f.write(content)
