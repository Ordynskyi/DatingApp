import { Photo } from "./photo"


export interface Member {
    id: number
    username: string
    photoUrl: string
    age: number
    displayName: string
    created: string
    lastActive: string
    introduction: string
    lookingFor: string
    city: string
    country: string
    photos: Photo[]
  }
  
