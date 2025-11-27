export interface Technology {
  id: string;
  name: string;
  description: string;
  tier: number;
  requiredTechnologies: string[];
}

export interface TechnologyWithStatus extends Technology {
  isOwned: boolean;
  canResearch: boolean;
  missingRequirements: string[];
}

export interface TechnologiesForTier {
  tier: number;
  technologies: TechnologyWithStatus[];
}

export interface FractionTechnology {
  technologyId: string;
  technologyName: string;
  source: 'Research' | 'Trade' | 'Exchange' | 'Other';
  sourceFractionId?: string;
  sourceFractionName?: string;
  sourceDescription?: string;
  acquiredDate: string;
}

export interface FractionGameState {
  id: string;
  fractionId: string;
  fractionName: string;
  currentTier: number;
  researchedTechnologiesInCurrentTier: number;
  canAdvanceToNextTier: boolean;
  technologies: FractionTechnology[];
  createdAt: string;
  updatedAt: string;
}

export interface AddTechnologyRequest {
  fractionId: string;
  technologyId: string;
  source: 'Research' | 'Trade' | 'Exchange' | 'Other';
  sourceFractionId?: string;
  sourceDescription?: string;
}

export interface GameSession {
  id: string;
  name: string;
  currentTurn: number;
  fractionIds: string[];
  status: string;
  createdAt: string;
  updatedAt: string;
}
