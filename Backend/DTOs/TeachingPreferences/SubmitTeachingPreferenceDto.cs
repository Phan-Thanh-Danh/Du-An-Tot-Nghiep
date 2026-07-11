using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TeachingPreferences;

public class SubmitTeachingPreferenceDto : UpdateTeachingPreferenceDto
{
    // Same as Update, but implies state transition to "submitted"
}
